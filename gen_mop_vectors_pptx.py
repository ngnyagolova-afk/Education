import sys
sys.stdout.reconfigure(encoding='utf-8')
from pathlib import Path
from pptx import Presentation
from pptx.util import Inches, Pt, Cm
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN
from pptx.util import Emu

# ── Цветова схема ──────────────────────────────────────────────────────────
C_DARK   = RGBColor(0x1A, 0x23, 0x7E)   # тъмно синьо
C_ACCENT = RGBColor(0x00, 0xB0, 0xFF)   # светло синьо
C_GREEN  = RGBColor(0x00, 0xC8, 0x53)   # зелено (код)
C_ORANGE = RGBColor(0xFF, 0x6D, 0x00)   # оранжево (акцент)
C_WHITE  = RGBColor(0xFF, 0xFF, 0xFF)
C_LIGHT  = RGBColor(0xE8, 0xEA, 0xFF)   # светъл фон
C_GRAY   = RGBColor(0x42, 0x42, 0x42)

prs = Presentation()
prs.slide_width  = Inches(13.33)
prs.slide_height = Inches(7.5)

BLANK = prs.slide_layouts[6]  # blank

def slide():
    return prs.slides.add_slide(BLANK)

def rect(sl, l, t, w, h, fill=None, line=None):
    s = sl.shapes.add_shape(1, Inches(l), Inches(t), Inches(w), Inches(h))
    s.line.fill.background()
    if fill:
        s.fill.solid(); s.fill.fore_color.rgb = fill
    else:
        s.fill.background()
    if line:
        s.line.color.rgb = line; s.line.width = Pt(1.5)
    else:
        s.line.fill.background()
    return s

def txb(sl, text, l, t, w, h, size=20, bold=False, color=C_GRAY,
        align=PP_ALIGN.LEFT, wrap=True):
    tb = sl.shapes.add_textbox(Inches(l), Inches(t), Inches(w), Inches(h))
    tf = tb.text_frame
    tf.word_wrap = wrap
    p = tf.paragraphs[0]
    p.alignment = align
    run = p.add_run()
    run.text = text
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    return tb

def header_bar(sl, title, subtitle=''):
    rect(sl, 0, 0, 13.33, 1.4, fill=C_DARK)
    rect(sl, 0, 1.4, 13.33, 0.08, fill=C_ACCENT)
    txb(sl, title, 0.4, 0.1, 12, 0.8, size=32, bold=True, color=C_WHITE, align=PP_ALIGN.LEFT)
    if subtitle:
        txb(sl, subtitle, 0.4, 0.85, 12, 0.5, size=16, color=C_ACCENT, align=PP_ALIGN.LEFT)

def code_box(sl, code, l, t, w, h):
    rect(sl, l, t, w, h, fill=RGBColor(0x1E, 0x1E, 0x1E), line=C_ACCENT)
    txb(sl, code, l+0.15, t+0.12, w-0.3, h-0.24,
        size=13, color=C_GREEN, align=PP_ALIGN.LEFT)

def bullet_box(sl, items, l, t, w, h, bg=C_LIGHT):
    rect(sl, l, t, w, h, fill=bg)
    tb = sl.shapes.add_textbox(Inches(l+0.2), Inches(t+0.15), Inches(w-0.4), Inches(h-0.3))
    tf = tb.text_frame; tf.word_wrap = True
    for i, (icon, text, sz) in enumerate(items):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.space_before = Pt(4)
        r = p.add_run()
        r.text = f'{icon}  {text}'
        r.font.size = Pt(sz)
        r.font.color.rgb = C_DARK

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 1 — Заглавен
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=C_DARK)
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0x1A, 0x23, 0x7E))

# Декоративни кръгове
for cx, cy, r_in, alpha in [(10.5,1.5,2.5,0x30),(11.5,5.5,3,0x25),(2,6.5,1.8,0x20)]:
    s = sl.shapes.add_shape(9, Inches(cx-r_in/2), Inches(cy-r_in/2),
                            Inches(r_in), Inches(r_in))
    s.fill.solid(); s.fill.fore_color.rgb = C_ACCENT
    s.line.fill.background()

rect(sl, 0.5, 2.2, 12.3, 0.06, fill=C_ACCENT)
txb(sl, 'МОП — Математика и Основи на Програмирането', 0.5, 1.2, 12, 0.6,
    size=18, color=C_ACCENT, align=PP_ALIGN.LEFT)
txb(sl, 'Връзка между вектори\nи масиви в програмирането', 0.5, 2.4, 12, 2.0,
    size=40, bold=True, color=C_WHITE, align=PP_ALIGN.LEFT)
txb(sl, 'Раздел 5  ·  11А клас  ·  C#', 0.5, 4.5, 8, 0.6,
    size=20, color=RGBColor(0xB3, 0xC5, 0xFF), align=PP_ALIGN.LEFT)
txb(sl, '← Предишен урок: Вектор. Свойства на векторите', 0.5, 5.3, 10, 0.5,
    size=15, color=RGBColor(0x90, 0xA4, 0xAE), align=PP_ALIGN.LEFT)

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 2 — Преговор: Вектор. Свойства
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0xF5, 0xF6, 0xFF))
header_bar(sl, '🔁 Преговор — Вектор. Свойства', 'Какво научихме миналия час?')

rect(sl, 0.4, 1.7, 5.8, 5.3, fill=C_WHITE, line=C_ACCENT)
txb(sl, '📐 Математически вектор', 0.6, 1.85, 5.4, 0.5, size=16, bold=True, color=C_DARK)
bullet_box(sl, [
    ('▸', 'Насочена отсечка с начало и край', 14),
    ('▸', 'Характеристики: посока, дължина (модул)', 14),
    ('▸', 'Означение: ā или AB', 14),
    ('▸', 'Координати: ā = (x, y) или (x, y, z)', 14),
    ('▸', 'Нулев вектор: (0, 0)', 14),
    ('▸', 'Единичен вектор (верзор): |ā| = 1', 14),
], 0.6, 2.4, 5.4, 4.3, bg=RGBColor(0xEE, 0xF0, 0xFF))

rect(sl, 6.8, 1.7, 6.1, 5.3, fill=C_WHITE, line=C_ORANGE)
txb(sl, '⚡ Свойства', 7.0, 1.85, 5.7, 0.5, size=16, bold=True, color=C_DARK)
bullet_box(sl, [
    ('✔', 'Комутативност: ā + b̄ = b̄ + ā', 14),
    ('✔', 'Асоциативност: (ā+b̄)+c̄ = ā+(b̄+c̄)', 14),
    ('✔', 'Нулев елемент: ā + 0̄ = ā', 14),
    ('✔', 'Умножение по скалар: k·ā', 14),
    ('✔', 'Скаларно произведение: ā·b̄ = |ā||b̄|cosθ', 14),
    ('✔', 'Колинеарност, перпендикулярност', 14),
], 7.0, 2.4, 5.7, 4.3, bg=RGBColor(0xFF, 0xF3, 0xE0))

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 3 — Мостът: от математика към код
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0xF5, 0xF6, 0xFF))
header_bar(sl, '🌉 Мостът — от математика към програмиране', '')

txb(sl, 'Математическият вектор (x, y, z) → масив в C#', 0.4, 1.6, 12, 0.5,
    size=20, bold=True, color=C_DARK, align=PP_ALIGN.CENTER)

# Ляво — математика
rect(sl, 0.4, 2.2, 5.6, 4.5, fill=RGBColor(0xE8, 0xEA, 0xFF), line=C_DARK)
txb(sl, '📐 Математика', 0.6, 2.35, 5.2, 0.45, size=17, bold=True, color=C_DARK)
txb(sl, 'ā = (3, -1, 5)\n\nКоординати: x=3, y=-1, z=5\n\nМодул: √(3²+1²+5²) = √35\n\nСкаларно произведение:\nā · b̄ = 3·1 + (-1)·2 + 5·0 = 1',
    0.6, 2.85, 5.2, 3.6, size=15, color=C_GRAY)

# Стрелка
txb(sl, '⟹', 6.1, 3.9, 1.1, 0.6, size=36, bold=True, color=C_ORANGE, align=PP_ALIGN.CENTER)

# Дясно — C#
rect(sl, 7.3, 2.2, 5.6, 4.5, fill=RGBColor(0x1E, 0x1E, 0x1E), line=C_ACCENT)
txb(sl, '{ } C# код', 7.5, 2.35, 5.2, 0.45, size=17, bold=True, color=C_ACCENT)
txb(sl,
    'double[] a = { 3, -1, 5 };\n\n// x=a[0], y=a[1], z=a[2]\n\n// Модул:\ndouble mod = Math.Sqrt(\n  a[0]*a[0]+a[1]*a[1]+a[2]*a[2]);\n\n// Скаларно произведение:\ndouble dot = a[0]*b[0]\n           + a[1]*b[1]\n           + a[2]*b[2];',
    7.5, 2.85, 5.2, 3.6, size=13, color=C_GREEN)

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 4 — Масивът е вектор!
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0xF5, 0xF6, 0xFF))
header_bar(sl, '💡 Масивът е вектор!', 'Паралел между понятията')

# Таблица с паралели
headers = ['Математически вектор', 'Масив в C#']
rows = [
    ('Размерност n', 'Дължина на масива (Length)'),
    ('Координата xᵢ', 'Елемент a[i]'),
    ('Нулев вектор (0,0,...)', 'new double[n] — нули по подразбиране'),
    ('Сумиране ā + b̄', 'Цикъл: c[i] = a[i] + b[i]'),
    ('Умножение k·ā', 'Цикъл: b[i] = k * a[i]'),
    ('Скаларно произведение', 'Цикъл с натрупване на a[i]*b[i]'),
    ('Модул |ā|', 'Math.Sqrt(Σ a[i]*a[i])'),
]

col_w = [5.8, 6.8]
x_start = [0.4, 6.4]
y = 1.65
rect(sl, 0.4, y, 12.6, 0.45, fill=C_DARK)
for c, h in enumerate(headers):
    txb(sl, h, x_start[c]+0.1, y+0.05, col_w[c]-0.2, 0.35,
        size=15, bold=True, color=C_WHITE)

for i, (m, p) in enumerate(rows):
    bg = RGBColor(0xEE, 0xF0, 0xFF) if i % 2 == 0 else C_WHITE
    rect(sl, 0.4, y+0.45+i*0.62, 12.6, 0.62, fill=bg)
    txb(sl, m, x_start[0]+0.1, y+0.48+i*0.62, col_w[0]-0.2, 0.52, size=14, color=C_DARK)
    txb(sl, p, x_start[1]+0.1, y+0.48+i*0.62, col_w[1]-0.2, 0.52, size=14, color=RGBColor(0x1A,0x5F,0x00))

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 5 — C# код: операции с вектори
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0x12, 0x12, 0x1A))
header_bar(sl, '{ } Операции с вектори в C#', 'Пишем математиката като код')

code1 = '''double[] VectorAdd(double[] a, double[] b) {
    double[] c = new double[a.Length];
    for (int i = 0; i < a.Length; i++)
        c[i] = a[i] + b[i];
    return c;
}'''

code2 = '''double DotProduct(double[] a, double[] b) {
    double sum = 0;
    for (int i = 0; i < a.Length; i++)
        sum += a[i] * b[i];
    return sum;
}'''

code3 = '''double Magnitude(double[] a) {
    double sum = 0;
    foreach (var x in a)
        sum += x * x;
    return Math.Sqrt(sum);
}'''

txb(sl, '➕ Събиране на вектори', 0.4, 1.55, 4.0, 0.4, size=14, bold=True, color=C_ACCENT)
code_box(sl, code1, 0.4, 2.0, 4.0, 2.3)

txb(sl, '· Скаларно произведение', 4.6, 1.55, 4.2, 0.4, size=14, bold=True, color=C_ACCENT)
code_box(sl, code2, 4.6, 2.0, 4.2, 2.3)

txb(sl, '📏 Модул (дължина)', 9.0, 1.55, 4.0, 0.4, size=14, bold=True, color=C_ACCENT)
code_box(sl, code3, 9.0, 2.0, 4.0, 2.3)

# Долна секция — защо е важно
rect(sl, 0.4, 4.5, 12.5, 2.7, fill=RGBColor(0x1A, 0x23, 0x7E))
txb(sl, '🎮 Защо го учим? — Реални приложения:', 0.6, 4.65, 12, 0.45,
    size=16, bold=True, color=C_ACCENT)
apps = [
    ('🎮', 'Игри: позиция, скорост, посока на персонажа'),
    ('🤖', 'Machine Learning: feature vectors, embeddings'),
    ('🗺️', 'GPS навигация: вектори на движение'),
    ('💹', 'Финанси: портфейли от активи = вектори'),
]
for i, (icon, text) in enumerate(apps):
    x = 0.6 + (i % 2) * 6.3
    y_pos = 5.2 + (i // 2) * 0.6
    txb(sl, f'{icon}  {text}', x, y_pos, 6.0, 0.55, size=14, color=C_WHITE)

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 6 — Защо ги учат? (мотивация)
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0xF5, 0xF6, 0xFF))
header_bar(sl, '🎯 Защо учим вектори в програмирането?', 'Мотивация и реален свят')

examples = [
    ('🎮', 'Игри (Unity / C#)', 'Vector3 position = new Vector3(x, y, z);\nVector3 velocity = new Vector3(dx, dy, dz);\n// Всеки game object използва вектори!'),
    ('🤖', 'Изкуствен интелект', 'Думите в ChatGPT са вектори!\n"цар" - "мъж" + "жена" = "царица"\nEmbedding = float[] с 1536 числа'),
    ('📊', 'Анализ на данни', 'Dataset = матрица от вектори\nКлъстеризация = разстояния между вектори\nNumPy, ML.NET работят с масиви'),
    ('🏎️', 'Физика в игри', 'Гравитация, тласъци, сблъсъци —\nвсичко е вектори!\nPhysics.Raycast(origin, direction)'),
]

for i, (icon, title, desc) in enumerate(examples):
    x = 0.4 + (i % 2) * 6.5
    y_pos = 1.6 + (i // 2) * 2.85
    col = C_DARK if i % 2 == 0 else RGBColor(0x4A, 0x00, 0x8A)
    rect(sl, x, y_pos, 6.1, 2.6, fill=C_WHITE, line=col)
    txb(sl, f'{icon}  {title}', x+0.15, y_pos+0.1, 5.8, 0.45, size=16, bold=True, color=col)
    txb(sl, desc, x+0.15, y_pos+0.6, 5.8, 1.9, size=13, color=C_GRAY)

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 7 — Unity демо: Vector3
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0x12, 0x12, 0x1A))
header_bar(sl, '🎮 Unity + C# — Vector3 в игрите', 'Вектори = живот на game object-ите')

txb(sl, 'Всеки обект в Unity има Transform с 3 вектора:', 0.4, 1.6, 12, 0.45,
    size=18, bold=True, color=C_WHITE)

unity_code = '''public class PlayerMovement : MonoBehaviour {
    public float speed = 5f;

    void Update() {
        // Вектор на движение от клавиатурата
        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal"),   // x
            0,                             // y
            Input.GetAxis("Vertical")      // z
        );

        // Движим се по вектора
        transform.position += direction * speed * Time.deltaTime;

        // Модулът показва скоростта
        float currentSpeed = direction.magnitude;
    }
}'''

code_box(sl, unity_code, 0.4, 2.1, 7.5, 5.1)

rect(sl, 8.1, 2.1, 4.9, 2.4, fill=RGBColor(0x1A, 0x23, 0x7E))
txb(sl, '🔑 Ключови понятия', 8.3, 2.2, 4.5, 0.4, size=15, bold=True, color=C_ACCENT)
bullet_box(sl, [
    ('▸', 'Vector3 = масив от 3 числа', 13),
    ('▸', '.magnitude = модул', 13),
    ('▸', '.normalized = единичен вектор', 13),
    ('▸', 'Vector3.Dot() = скаларно произведение', 13),
    ('▸', 'Vector3.Cross() = векторно произведение', 13),
], 8.3, 2.65, 4.6, 4.3, bg=RGBColor(0x1E, 0x27, 0x5E))

rect(sl, 8.1, 4.6, 4.9, 2.6, fill=RGBColor(0x1E, 0x1E, 0x1E), line=C_GREEN)
txb(sl, 'Vector3 a = new Vector3(1,0,0);\nVector3 b = new Vector3(0,1,0);\nfloat dot   = Vector3.Dot(a,b);   // 0\nVector3 sum = a + b;  // (1,1,0)',
    8.25, 4.72, 4.6, 2.3, size=12, color=C_GREEN)

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 8 — Упражнение (задача)
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0xF5, 0xF6, 0xFF))
header_bar(sl, '✏️ Упражнение — Напишете сами!', 'Задача за клас')

rect(sl, 0.4, 1.6, 12.5, 2.5, fill=RGBColor(0xFF, 0xF3, 0xE0), line=C_ORANGE)
txb(sl, '📋 Задача:', 0.6, 1.72, 3, 0.4, size=17, bold=True, color=C_ORANGE)
txb(sl, 'Напишете C# метод, който приема два вектора (масиви) и:\n  1. Изчислява и принтира тяхната сума\n  2. Изчислява и принтира скаларното им произведение\n  3. Проверява дали са перпендикулярни (dot product = 0)',
    0.6, 2.15, 12, 1.8, size=15, color=C_GRAY)

txb(sl, '💡 Помощен шаблон:', 0.4, 4.25, 5, 0.4, size=15, bold=True, color=C_DARK)
code_box(sl, 'static void VectorInfo(double[] a, double[] b) {\n    // Вашият код тук...\n    // Hint: използвайте for цикъл!\n}',
         0.4, 4.7, 6.2, 2.5)

rect(sl, 6.8, 4.25, 6.1, 2.95, fill=RGBColor(0xE8, 0xF5, 0xE9), line=C_GREEN)
txb(sl, '✅ Тест данни:', 7.0, 4.38, 5.7, 0.4, size=15, bold=True, color=C_GREEN)
txb(sl, 'a = { 3, 4, 0 }\nb = { -4, 3, 0 }\n\nОчакван резултат:\n  Сума: { -1, 7, 0 }\n  Dot product: 0\n  Перпендикулярни: ДА ✔',
    7.0, 4.85, 5.7, 2.2, size=14, color=C_GRAY)

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 9 — Дигитални игри и ресурси
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0xF5, 0xF6, 0xFF))
header_bar(sl, '🎯 Дигитални игри и интерактивни ресурси', 'Учете се играейки!')

games = [
    ('🎮', 'GeoGebra — Вектори', 'geogebra.org', 'Интерактивни вектори в 2D/3D пространство. Движете и събирайте вектори визуално.', C_DARK),
    ('🧩', 'Brilliant.org', 'brilliant.org', 'Курс "Linear Algebra" с интерактивни задачи и визуализации стъпка по стъпка.', RGBColor(0x6A,0x1B,0x9A)),
    ('🕹️', 'Unity Learn', 'learn.unity.com', '"Create with Code" — правите игра с вектори в C#! Безплатно, с награди.', RGBColor(0x00,0x7A,0x36)),
    ('⚡', 'Khan Academy', 'bg.khanacademy.org', 'Видео уроци по вектори с автоматична проверка на задачите. На български!', RGBColor(0xB7,0x1C,0x1C)),
    ('🎲', 'Kahoot — Вектори', 'kahoot.com', 'Търсете готови Kahoot викторини по вектори или създайте своя за класа!', C_ORANGE),
    ('💻', 'Scratch + вектори', 'scratch.mit.edu', 'Направете анимация с движещи се обекти — векторите оживяват!', RGBColor(0x00,0x70,0xA0)),
]

for i, (icon, name, url, desc, col) in enumerate(games):
    x = 0.4 + (i % 3) * 4.3
    y_pos = 1.6 + (i // 3) * 2.85
    rect(sl, x, y_pos, 4.0, 2.65, fill=C_WHITE, line=col)
    txb(sl, f'{icon} {name}', x+0.12, y_pos+0.1, 3.75, 0.45, size=15, bold=True, color=col)
    txb(sl, url, x+0.12, y_pos+0.58, 3.75, 0.3, size=11, color=C_ACCENT)
    txb(sl, desc, x+0.12, y_pos+0.92, 3.75, 1.6, size=12, color=C_GRAY)

# ════════════════════════════════════════════════════════════════════════════
# СЛАЙД 10 — Обобщение
# ════════════════════════════════════════════════════════════════════════════
sl = slide()
rect(sl, 0, 0, 13.33, 7.5, fill=C_DARK)
rect(sl, 0, 0, 13.33, 7.5, fill=RGBColor(0x1A, 0x23, 0x7E))
rect(sl, 0, 0, 13.33, 0.08, fill=C_ACCENT)
rect(sl, 0, 7.42, 13.33, 0.08, fill=C_ACCENT)

txb(sl, '📌 Обобщение', 0.5, 0.6, 12, 0.6, size=28, bold=True, color=C_ACCENT, align=PP_ALIGN.CENTER)

summary = [
    ('🔢', 'Векторът (x₁, x₂, ..., xₙ) = масив double[] в C#'),
    ('➕', 'Операциите събиране, скаларно произведение, модул → for цикли'),
    ('🎮', 'Unity Vector3 е директна реализация на 3D вектор'),
    ('🤖', 'ML, физика, игри, навигация — всичко работи с вектори'),
    ('💡', 'Математиката не е абстрактна — тя е в кода, който пишете!'),
]

for i, (icon, text) in enumerate(summary):
    y_pos = 1.4 + i * 1.0
    rect(sl, 0.8, y_pos, 11.7, 0.82, fill=RGBColor(0x22, 0x2D, 0x8E))
    txb(sl, f'{icon}   {text}', 1.0, y_pos+0.12, 11.3, 0.6, size=17, color=C_WHITE)

txb(sl, 'Следващ урок: Операции с вектори — задачи ▶',
    0.5, 6.9, 12, 0.45, size=14, color=RGBColor(0x90, 0xA4, 0xAE), align=PP_ALIGN.CENTER)

# ── Запис ───────────────────────────────────────────────────────────────────
out = Path(r'C:\Users\Neli Nqgolova\Documents\Education\11а клас\МОП\Раздел 5\Вектори_и_масиви_C#.pptx')
out.parent.mkdir(parents=True, exist_ok=True)
prs.save(str(out))
print(f'✅ Презентацията е записана:\n   {out}')
print(f'\n📊 Слайдове: 10')
print(f'📁 Папка: Раздел 5')
