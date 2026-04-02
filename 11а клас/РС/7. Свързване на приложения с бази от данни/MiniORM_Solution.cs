// ============================================================
//  MiniORM — пълно решение (готово за копиране в VS 2022+)
//  Класове: ChangeTracker<T>  •  DbSet<TEntity>  •  DbContext
//  Бележка: DatabaseConnection, ConnectionManager и
//           ReflectionHelper се намират в скелета на проекта.
// ============================================================

namespace MiniORM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Reflection;

    // ──────────────────────────────────────────────────────────
    //  Помощен extension метод  pi.HasAttribute<T>()
    // ──────────────────────────────────────────────────────────
    internal static class AttributeExtensions
    {
        public static bool HasAttribute<TAttribute>(this PropertyInfo pi)
            where TAttribute : Attribute
            => pi.GetCustomAttribute<TAttribute>() != null;
    }

    // ──────────────────────────────────────────────────────────
    //  ChangeTracker<T>
    //  Следи добавени, премахнати и модифицирани обекти
    // ──────────────────────────────────────────────────────────
    public class ChangeTracker<T> where T : class, new()
    {
        private readonly List<T> allEntities;
        private readonly List<T> added;
        private readonly List<T> removed;

        public IReadOnlyCollection<T> AllEntities => allEntities.AsReadOnly();
        public IReadOnlyCollection<T> Added       => added.AsReadOnly();
        public IReadOnlyCollection<T> Removed     => removed.AsReadOnly();

        public ChangeTracker(IEnumerable<T> entities)
        {
            allEntities = new List<T>();
            added       = new List<T>();
            removed     = new List<T>();
            CloneEntities(entities);
        }

        // Прави snapshot на оригиналните стойности (само SQL полета)
        private void CloneEntities(IEnumerable<T> entities)
        {
            var clonedProps = typeof(T)
                .GetProperties()
                .Where(pi => DbContext.AllowedSqlTypes.Contains(pi.PropertyType));

            foreach (var entity in entities)
            {
                var clone = Activator.CreateInstance<T>();
                foreach (var prop in clonedProps)
                    prop.SetValue(clone, prop.GetValue(entity));
                allEntities.Add(clone);
            }
        }

        public void Add(T item)    => added.Add(item);
        public void Remove(T item) => removed.Add(item);

        // Връща обектите, чиито стойности са се променили
        public IEnumerable<T> GetModifiedEntities(DbSet<T> dbSet)
        {
            foreach (var proxyEntity in dbSet)
            {
                var pkValues    = GetPrimaryKeyValues(proxyEntity).ToArray();
                var origEntity  = allEntities
                    .SingleOrDefault(e =>
                        GetPrimaryKeyValues(e).SequenceEqual(pkValues));

                if (origEntity != null && IsModified(origEntity, proxyEntity))
                    yield return proxyEntity;
            }
        }

        // Сравнява snapshot с текущия обект
        private bool IsModified(T orig, T proxy)
        {
            var props = typeof(T)
                .GetProperties()
                .Where(pi => DbContext.AllowedSqlTypes.Contains(pi.PropertyType));

            return props.Any(pi =>
                !Equals(pi.GetValue(orig), pi.GetValue(proxy)));
        }

        // Взима стойностите на [Key] свойствата
        private IEnumerable<object> GetPrimaryKeyValues(T entity)
        {
            return typeof(T)
                .GetProperties()
                .Where(pi => pi.HasAttribute<KeyAttribute>())
                .Select(pi => pi.GetValue(entity));
        }
    }

    // ──────────────────────────────────────────────────────────
    //  DbSet<TEntity>
    //  Колекция от данни от един тип — представя таблица в БД
    // ──────────────────────────────────────────────────────────
    public class DbSet<TEntity> : ICollection<TEntity>
        where TEntity : class, new()
    {
        private readonly ChangeTracker<TEntity> changeTracker;

        public ChangeTracker<TEntity>           ChangeTracker => changeTracker;
        public IReadOnlyCollection<TEntity>     Entities      => changeTracker.AllEntities;

        // Конструкторът е internal — само DbContext го извиква
        internal DbSet(IEnumerable<TEntity> entities)
        {
            changeTracker = new ChangeTracker<TEntity>(entities);
        }

        // ── ICollection<TEntity> ─────────────────────────────
        public void Add(TEntity item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            changeTracker.Add(item);
        }

        public bool Remove(TEntity item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            changeTracker.Remove(item);
            return true;
        }

        public void Clear()
        {
            // Ползваме Remove за всеки елемент, за да регистрира ChangeTracker
            var all = changeTracker.AllEntities.ToList();
            foreach (var item in all)
                Remove(item);
        }

        public bool Contains(TEntity item)
            => changeTracker.AllEntities.Contains(item);

        public void CopyTo(TEntity[] array, int arrayIndex)
            => changeTracker.AllEntities.CopyTo(array, arrayIndex);

        public int  Count      => changeTracker.AllEntities.Count;
        public bool IsReadOnly => false;

        public IEnumerator<TEntity> GetEnumerator()
            => changeTracker.AllEntities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        // Допълнителен метод — масово премахване
        public void RemoveRange(IEnumerable<TEntity> items)
        {
            foreach (var item in items)
                Remove(item);
        }
    }

    // ──────────────────────────────────────────────────────────
    //  DbContext  (abstract)
    //  Централен клас — свързва всичко и управлява SaveChanges
    // ──────────────────────────────────────────────────────────
    public abstract class DbContext
    {
        private readonly DatabaseConnection           dbConnection;
        private readonly Dictionary<Type, PropertyInfo> dbSetProperties;

        // Типове, разрешени като SQL колони
        internal static readonly Type[] AllowedSqlTypes =
        {
            typeof(string),  typeof(int),      typeof(uint),
            typeof(long),    typeof(ulong),    typeof(decimal),
            typeof(bool),    typeof(DateTime),
        };

        protected DbContext(string connectionString)
        {
            dbConnection    = DatabaseConnection.Instance(connectionString);
            dbSetProperties = DiscoverDbSets();

            using (new ConnectionManager(dbConnection))
            {
                InitializeDbSets();
            }

            MapAllRelations();
        }

        // ── Единственият public метод ─────────────────────────
        public void SaveChanges()
        {
            // Валидация на всички данни преди запис
            foreach (var dbSetProp in dbSetProperties.Values)
            {
                var dbSetVal    = dbSetProp.GetValue(this);
                var entityType  = dbSetProp.PropertyType.GetGenericArguments().First();

                var entities = ((IEnumerable)dbSetVal).Cast<object>().ToArray();
                var invalid  = entities.Where(e => !IsObjectValid(e)).ToArray();

                if (invalid.Length > 0)
                    throw new InvalidOperationException(
                        $"{invalid.Length} Invalid Entities found in {dbSetProp.Name}!");
            }

            using (new ConnectionManager(dbConnection))
            {
                using var transaction = dbConnection.StartTransaction();
                try
                {
                    foreach (var dbSetProp in dbSetProperties.Values)
                    {
                        var entityType = dbSetProp.PropertyType.GetGenericArguments().First();
                        var persistMethod = typeof(DbContext)
                            .GetMethod(nameof(Persist),
                                BindingFlags.Instance | BindingFlags.NonPublic)!
                            .MakeGenericMethod(entityType);

                        try
                        {
                            persistMethod.Invoke(this, new[] { dbSetProp.GetValue(this) });
                        }
                        catch (TargetInvocationException tie)
                        {
                            throw tie.InnerException ?? tie;
                        }
                    }

                    transaction.Commit();
                }
                catch (InvalidOperationException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (System.Data.SqlClient.SqlException)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // ── INSERT / UPDATE / DELETE за един DbSet<TEntity> ──
        private void Persist<TEntity>(DbSet<TEntity> dbSet)
            where TEntity : class, new()
        {
            var tableName = GetTableName(typeof(TEntity));
            var columns   = GetEntityColumnNames(typeof(TEntity)).ToArray();
            var tracker   = dbSet.ChangeTracker;

            foreach (var entity in tracker.Added)
                dbConnection.InsertEntity(entity, tableName, columns);

            foreach (var entity in tracker.Removed)
                dbConnection.DeleteEntity(entity, tableName, columns);

            foreach (var entity in tracker.GetModifiedEntities(dbSet))
                dbConnection.UpdateEntity(entity, tableName, columns);
        }

        // ── Зареждане на DbSet-овете от БД ───────────────────
        private void InitializeDbSets()
        {
            foreach (var dbSetProp in dbSetProperties)
            {
                var entityType = dbSetProp.Value.PropertyType.GetGenericArguments().First();

                var method = typeof(DbContext)
                    .GetMethod(nameof(LoadTableEntities),
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(entityType);

                var dbSetInstance = method.Invoke(this, null);
                dbSetProp.Value.SetValue(this, dbSetInstance);
            }
        }

        private DbSet<TEntity> LoadTableEntities<TEntity>()
            where TEntity : class, new()
        {
            var table    = GetTableName(typeof(TEntity));
            var columns  = GetEntityColumnNames(typeof(TEntity)).ToArray();
            var entities = dbConnection.FetchResultsTable<TEntity>(table, columns);
            return new DbSet<TEntity>(entities);
        }

        // ── Свързване на навигационни свойства ───────────────
        private void MapAllRelations()
        {
            foreach (var dbSetProp in dbSetProperties)
            {
                var entityType = dbSetProp.Value.PropertyType.GetGenericArguments().First();

                var method = typeof(DbContext)
                    .GetMethod(nameof(MapRelations),
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(entityType);

                method.Invoke(this, new[] { dbSetProp.Value.GetValue(this) });
            }
        }

        private void MapRelations<TEntity>(DbSet<TEntity> dbSet)
            where TEntity : class, new()
        {
            foreach (var entity in dbSet)
                MapNavigationProperties(dbSet, entity);
        }

        private void MapNavigationProperties<TEntity>(DbSet<TEntity> dbSet, TEntity entity)
            where TEntity : class, new()
        {
            var entityType = typeof(TEntity);

            // Many-to-One: свойства с [ForeignKey] атрибут
            var fkProps = entityType
                .GetProperties()
                .Where(pi => pi.HasAttribute<ForeignKeyAttribute>());

            foreach (var fkProp in fkProps)
            {
                var navPropName = fkProp.GetCustomAttribute<ForeignKeyAttribute>()!.Name;
                var navProp     = entityType.GetProperty(navPropName);
                if (navProp == null) continue;

                var navType = navProp.PropertyType;
                if (!dbSetProperties.ContainsKey(navType)) continue;

                var navDbSet  = dbSetProperties[navType].GetValue(this);
                var fkValue   = fkProp.GetValue(entity);

                var pkProp = navType.GetProperties()
                    .FirstOrDefault(pi => pi.HasAttribute<KeyAttribute>());
                if (pkProp == null) continue;

                var related = ((IEnumerable)navDbSet)
                    .Cast<object>()
                    .FirstOrDefault(e => Equals(pkProp.GetValue(e), fkValue));

                ReflectionHelper.ReplaceBackingField(entity, navPropName, related);
            }

            // One-to-Many / Many-to-Many: ICollection<T> свойства
            var collectionProps = entityType
                .GetProperties()
                .Where(pi => pi.PropertyType.IsGenericType &&
                             pi.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));

            foreach (var collProp in collectionProps)
            {
                var colType = collProp.PropertyType.GetGenericArguments().First();
                if (!dbSetProperties.ContainsKey(colType)) continue;

                var mapMethod = typeof(DbContext)
                    .GetMethod(nameof(MapCollection),
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(entityType, colType);

                mapMethod.Invoke(this, new object[] { dbSet, entity, collProp });
            }
        }

        private void MapCollection<TDbSet, TCollection>(
            DbSet<TDbSet> dbSet, TDbSet entity, PropertyInfo collectionProp)
            where TDbSet     : class, new()
            where TCollection : class, new()
        {
            var entityType     = typeof(TDbSet);
            var collectionType = typeof(TCollection);

            var primaryKeys = collectionType
                .GetProperties()
                .Where(pi => pi.HasAttribute<KeyAttribute>())
                .ToArray();

            var mappingKeys = primaryKeys
                .Where(pk => entityType
                    .GetProperties()
                    .Any(pi => pi.HasAttribute<KeyAttribute>() &&
                               pi.PropertyType == pk.PropertyType))
                .ToArray();

            // М:N ако junction table-ът има 2 PK-а
            if (mappingKeys.Length != 2) return;

            var entityPk = entityType.GetProperties()
                .FirstOrDefault(pi => pi.HasAttribute<KeyAttribute>());
            if (entityPk == null) return;

            var entityPkValue  = entityPk.GetValue(entity);
            var mappingKeyProp = mappingKeys
                .FirstOrDefault(mk => mk.PropertyType == entityPk.PropertyType);
            if (mappingKeyProp == null) return;

            var colDbSet = (IEnumerable<TCollection>)
                dbSetProperties[collectionType].GetValue(this)!;

            var related = colDbSet
                .Where(ce => Equals(mappingKeyProp.GetValue(ce), entityPkValue))
                .ToHashSet();

            ReflectionHelper.ReplaceBackingField(entity, collectionProp.Name, related);
        }

        // ── Помощни методи ────────────────────────────────────
        private Dictionary<Type, PropertyInfo> DiscoverDbSets()
        {
            return GetType()
                .GetProperties()
                .Where(pi => pi.PropertyType.IsGenericType &&
                             pi.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToDictionary(
                    pi => pi.PropertyType.GetGenericArguments().First(),
                    pi => pi);
        }

        private string GetTableName(Type t)
        {
            var attr = t.GetCustomAttribute<TableAttribute>();
            return attr != null ? attr.Name : t.Name + "s";
        }

        private IEnumerable<string> GetEntityColumnNames(Type t)
        {
            return t.GetProperties()
                .Where(pi => AllowedSqlTypes.Contains(pi.PropertyType) &&
                             !pi.HasAttribute<NotMappedAttribute>())
                .Select(pi => pi.HasAttribute<ColumnAttribute>()
                    ? pi.GetCustomAttribute<ColumnAttribute>()!.Name
                    : pi.Name);
        }

        private bool IsObjectValid(object obj)
        {
            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(
                obj, context, results, validateAllProperties: true);
        }
    }
}
