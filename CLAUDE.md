# Claude Rules for this Project

## Език на комуникация

**Всички отговори и комуникация с потребителя да са на български език.**
Програмните термини, команди, имена на инструменти и код се оставят на оригиналния им (английски) програмен език — например: `declare`, `shift`, `getopts`, `array`, `string`, `export`, bash, Python, и др.

## Output File Format

**Never generate `.md` files as text output files.**
When creating lesson plans, schemas, documents, or any other text output, always generate `.docx` format using `python-docx`.

- Use the Python installation at `C:\Users\Neli Nqgolova\AppData\Local\Programs\Python\Python313\python.exe`
- `python-docx` is already installed
- Generate `.docx` files directly — do not create `.md` as an intermediate step
