/** 
* !ACTIVIDAD PRENSENTADA POR 
**/
//? Marcos Jacome
//* Luis Brieva

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ============================================================
// BASE DE DATOS SIMULADA (en memoria) - Escuela Primaria
// ============================================================

var hoy = DateOnly.FromDateTime(DateTime.Today);

var estudiantes = new List<Estudiante>
{
    new() { Id = 1, Nombre = "Sofía Ramírez",   Edad = 7,  FechaNacimiento = hoy.AddYears(-7),  Sexo = "F", Alergias = "Maní",    Promedio = 4.2, EstadoSalud = "Estable",        Grado = "2do",  Seccion = "A", Matriculado = true,  Tutor = "DOC-001" },
    new() { Id = 2, Nombre = "Mateo González",  Edad = 10, FechaNacimiento = hoy.AddYears(-10), Sexo = "M", Alergias = "Ninguna", Promedio = 3.8, EstadoSalud = "En observación", Grado = "5to",  Seccion = "B", Matriculado = true,  Tutor = "DOC-002" },
    new() { Id = 3, Nombre = "Valentina Cruz",  Edad = 5,  FechaNacimiento = hoy.AddYears(-5),  Sexo = "F", Alergias = "Polen",   Promedio = 4.7, EstadoSalud = "Estable",        Grado = "1ro",  Seccion = "A", Matriculado = false, Tutor = null },
    new() { Id = 4, Nombre = "Diego López",     Edad = 12, FechaNacimiento = hoy.AddYears(-12), Sexo = "M", Alergias = "Lácteos",  Promedio = 2.9, EstadoSalud = "Estable",        Grado = "6to",  Seccion = "A", Matriculado = true,  Tutor = "DOC-001" }
};

var docentes = new List<Docente>
{
    new() { Codigo = "DOC-001", Nombre = "María Duarte",   Edad = 45, Email = "maria.duarte@colegio.com",   Rol = "Docente" },
    new() { Codigo = "DOC-002", Nombre = "Jorge Salazar",  Edad = 38, Email = "jorge.salazar@colegio.com",  Rol = "Docente" },
    new() { Codigo = "DIR-001", Nombre = "Elena Vargas",   Edad = 52, Email = "elena.vargas@colegio.com",   Rol = "Director" },
    new() { Codigo = "ADM-001", Nombre = "Raúl Medina",    Edad = 40, Email = "raul.medina@colegio.com",    Rol = "Admin" }
};

var matriculas = new List<Matricula>
{
    new() { Id = 1, EstudianteId = 2, CodigoDocente = "DOC-002", Grado = "5to", Seccion = "B", Fecha = hoy.AddDays(-3), Estado = "Activa" }
};

var siguienteIdEstudiante = 5;
var siguienteIdMatricula = 2;

// ============================================================
// AYUDAS
// ============================================================

IResult Error(int estado, string mensaje) =>
    Results.Json(new Error { Estado = estado, Mensaje = mensaje }, statusCode: estado);

static bool EsEmailValido(string email)
{
    var arroba = email.IndexOf('@');

    if (arroba <= 0 || arroba != email.LastIndexOf('@') || arroba == email.Length - 1)
        return false;

    var dominio = email[(arroba + 1)..];
    var punto = dominio.IndexOf('.');

    return punto > 0 && punto < dominio.Length - 1 && !dominio.Contains(' ');
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    app.Logger.LogError("Error interno: {error}", context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error);

    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(
        new Error { Estado = 500, Mensaje = "Ocurrió un fallo no controlado en la lógica interna de la API." });
}));

// Archivos estáticos (wwwroot/index.html).
app.UseStaticFiles();

// ============================================================
// 200 OK - Documentación de la API
// GET /
// ============================================================
app.MapGet("/", (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath!, "index.html"), "text/html"));

// ============================================================
// 200 OK - Listar estudiantes
// GET /api/estudiantes?matriculado=true
// ============================================================
app.MapGet("/api/estudiantes", (bool? matriculado, string? grado) =>
{
    var lista = estudiantes;

    if (matriculado is not null)
        lista = [.. lista.Where(e => e.Matriculado == matriculado.Value)];

    if (!string.IsNullOrWhiteSpace(grado))
        lista = [.. lista.Where(e => e.Grado.Equals(grado.Trim(), StringComparison.OrdinalIgnoreCase))];

    return Results.Ok(lista);
});

// ============================================================
// 400 / 422 / 201 - Registrar un estudiante
// POST /api/estudiantes
// ============================================================
app.MapPost("/api/estudiantes", (EstudianteDatos datos) =>
{
    // 400: al cliente le faltan datos obligatorios
    if (string.IsNullOrWhiteSpace(datos.Nombre))
        return Error(400, "El campo 'nombre' es obligatorio y no puede estar vacío.");

    if (datos.Edad is null)
        return Error(400, "El campo 'edad' es obligatorio.");

    if (string.IsNullOrWhiteSpace(datos.Grado))
        return Error(400, "El campo 'grado' es obligatorio.");

    // 422: el JSON esta bien escrito, pero el dato no cumple las reglas de la escuela
    if (datos.Edad is < 5 or > 12)
        return Error(422, "En la escuela primaria la edad debe estar entre 5 y 12 años.");

    if (datos.FechaNacimiento is not null && datos.FechaNacimiento > hoy)
        return Error(422, "La fecha de nacimiento no puede estar en el futuro.");

    if (datos.Promedio is < 0 or > 5)
        return Error(422, "El promedio escolar debe estar entre 0.0 y 5.0.");

    var sexo = (datos.Sexo ?? "X").ToUpper();
    if (sexo is not ("M" or "F" or "X"))
        return Error(422, "El sexo debe ser 'M', 'F' o 'X'.");

    // 201: todo correcto, se simula el guardado
    var estudiante = new Estudiante
    {
        Id = siguienteIdEstudiante++,
        Nombre = datos.Nombre.Trim(),
        Edad = datos.Edad.Value,
        FechaNacimiento = datos.FechaNacimiento ?? hoy.AddYears(-datos.Edad.Value),
        Sexo = sexo,
        Alergias = string.IsNullOrWhiteSpace(datos.Alergias) ? "Ninguna" : datos.Alergias,
        Promedio = datos.Promedio ?? 0,
        EstadoSalud = "Estable",
        Grado = datos.Grado.Trim(),
        Seccion = string.IsNullOrWhiteSpace(datos.Seccion) ? "A" : datos.Seccion.Trim().ToUpper(),
        Matriculado = true
    };
    estudiantes.Add(estudiante);

    return Results.Created($"/api/estudiantes/{estudiante.Id}", estudiante);
});

// ============================================================
// 200 / 404 - Buscar un estudiante por id
// GET /api/estudiantes/{id}
// ============================================================
app.MapGet("/api/estudiantes/{id}", (int id) =>
{
    // Se simula la consulta a la base de datos por id
    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);

    if (estudiante is null)
        return Error(404, $"No existe ningún estudiante con el id {id}.");

    return Results.Ok(estudiante);
});

// ============================================================
// 401 - Expediente académico (exige encabezado Authorization)
// GET /api/estudiantes/{id}/expediente
// ============================================================
app.MapGet("/api/estudiantes/{id}/expediente", (int id, HttpRequest request) =>
{
    if (!request.Headers.ContainsKey("Authorization"))
        return Error(401, "Falta el encabezado 'Authorization'. Envíalo para consultar el expediente académico.");

    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);

    if (estudiante is null)
        return Error(404, $"No existe ningún estudiante con el id {id}.");

    return Results.Ok(new
    {
        estudiante.Id,
        estudiante.Nombre,
        estudiante.Edad,
        estudiante.Grado,
        estudiante.Seccion,
        estudiante.EstadoSalud,
        estudiante.Alergias,
        estudiante.Matriculado,
        estudiante.Tutor,
        SolicitadoPor = request.Headers["Authorization"].ToString()
    });
});

// ============================================================
// 401 / 403 / 404 / 204 - Dar de baja a un estudiante de la escuela
// DELETE /api/estudiantes/{id}
// ============================================================
app.MapDelete("/api/estudiantes/{id}", (int id, HttpRequest request) =>
{
    // 401: el cliente no se identificó
    if (!request.Headers.ContainsKey("Authorization"))
        return Error(401, "Falta el encabezado 'Authorization'. Sin credenciales no se puede dar de baja a un estudiante.");

    // 403: el cliente sí se identificó, pero su rol no tiene permiso
    var rol = request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    if (rol != "Docente" && rol != "Director" && rol != "Admin")
        return Error(403, $"El rol '{rol}' no puede dar de baja estudiantes. Se requiere el rol 'Docente', 'Director' o 'Admin'.");

    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);

    if (estudiante is null)
        return Error(404, $"No existe ningún estudiante con el id {id}.");

    estudiantes.Remove(estudiante);

    // 204: la operación se hizo bien y el cuerpo va completamente vacío
    return Results.NoContent();
});

// ============================================================
// 500 - Fallo interno no controlado
// GET /api/estudiantes/{id}/promedio
// ============================================================
app.MapGet("/api/estudiantes/{id}/promedio", (int id) =>
{
    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);

    if (estudiante is null)
        return Error(404, $"No existe ningún estudiante con el id {id}.");

    var suma = 42;
    var notasEvaluadas = 0;

    var promedio = suma / notasEvaluadas;

    return Results.Ok(new { id, promedio });
});

// ============================================================
// 200 - Listar docentes
// GET /api/docentes
// ============================================================
app.MapGet("/api/docentes", (string? rol) =>
{
    var lista = docentes;

    if (!string.IsNullOrWhiteSpace(rol))
        lista = [.. lista.Where(d => d.Rol.Equals(rol.Trim(), StringComparison.OrdinalIgnoreCase))];

    return Results.Ok(lista);
});

// ============================================================
// 400 / 409 / 422 / 201 - Registrar un docente
// POST /api/docentes
// ============================================================
app.MapPost("/api/docentes", (DocenteDatos datos) =>
{
    // 400: al cliente le faltan datos obligatorios
    if (string.IsNullOrWhiteSpace(datos.Codigo))
        return Error(400, "El campo 'codigo' es obligatorio y no puede estar vacío.");

    if (string.IsNullOrWhiteSpace(datos.Nombre))
        return Error(400, "El campo 'nombre' es obligatorio y no puede estar vacío.");

    if (datos.Edad is null)
        return Error(400, "El campo 'edad' es obligatorio.");

    var codigo = datos.Codigo.Trim().ToUpper();

    // 409: el valor único ya existe
    if (docentes.Any(d => d.Codigo == codigo))
        return Error(409, $"Ya existe un docente con el código '{codigo}'. El código es único.");

    // 422: el JSON está bien escrito, pero el dato no cumple las reglas
    if (datos.Edad < 21)
        return Error(422, "El docente debe ser mayor de 21 años.");

    var email = datos.Email?.Trim() ?? "";
    if (email.Length > 0 && !EsEmailValido(email))
        return Error(422, "El email no tiene un formato válido.");

    // 201: todo correcto, se simula el guardado
    var docente = new Docente
    {
        Codigo = codigo,
        Nombre = datos.Nombre.Trim(),
        Edad = datos.Edad.Value,
        Email = email,
        Rol = string.IsNullOrWhiteSpace(datos.Rol) ? "Docente" : datos.Rol.Trim()
    };
    docentes.Add(docente);

    return Results.Created($"/api/docentes/{docente.Codigo}", docente);
});

// ============================================================
// 400 / 404 / 409 / 422 / 201 - Matricular un estudiante en un grado
// POST /api/matriculas
// ============================================================
app.MapPost("/api/matriculas", (MatriculaDatos datos) =>
{
    // 400: al cliente le faltan datos obligatorios
    if (datos.EstudianteId is null)
        return Error(400, "El campo 'estudianteId' es obligatorio.");

    if (string.IsNullOrWhiteSpace(datos.CodigoDocente))
        return Error(400, "El campo 'codigoDocente' es obligatorio.");

    if (string.IsNullOrWhiteSpace(datos.Grado))
        return Error(400, "El campo 'grado' es obligatorio.");

    // 404: el estudiante o el docente no existen
    var estudiante = estudiantes.FirstOrDefault(e => e.Id == datos.EstudianteId.Value);
    if (estudiante is null)
        return Error(404, $"No existe ningún estudiante con el id {datos.EstudianteId}.");

    var docente = docentes.FirstOrDefault(d => d.Codigo == datos.CodigoDocente.Trim().ToUpper());
    if (docente is null)
        return Error(404, $"No existe ningún docente con el código '{datos.CodigoDocente}'.");

    // 409: el estudiante ya tiene una matrícula activa
    if (estudiante.Matriculado)
        return Error(409, $"'{estudiante.Nombre}' ya está matriculado en {estudiante.Grado} {estudiante.Seccion}.");

    // 422: el JSON está bien escrito, pero el dato no cumple las reglas
    if (datos.Fecha is not null && datos.Fecha > hoy)
        return Error(422, "La fecha de matrícula no puede estar en el futuro.");

    if (docente.Edad < 21)
        return Error(422, $"El docente '{docente.Codigo}' no cumple la edad mínima de 21 años.");

    // 201: todo correcto, se simula el guardado
    var matricula = new Matricula
    {
        Id = siguienteIdMatricula++,
        EstudianteId = estudiante.Id,
        CodigoDocente = docente.Codigo,
        Grado = datos.Grado.Trim(),
        Seccion = string.IsNullOrWhiteSpace(datos.Seccion) ? "A" : datos.Seccion.Trim().ToUpper(),
        Fecha = datos.Fecha ?? hoy,
        Estado = "Pendiente"
    };
    matriculas.Add(matricula);

    estudiante.Matriculado = true;
    estudiante.Grado = matricula.Grado;
    estudiante.Seccion = matricula.Seccion;
    estudiante.Tutor = docente.Codigo;

    return Results.Created($"/api/matriculas/{matricula.Id}", matricula);
});

// ============================================================
// 200 / 404 / 204 - Anular una matrícula
// GET /api/matriculas
// DELETE /api/matriculas/{id}
// ============================================================
app.MapGet("/api/matriculas", () => Results.Ok(matriculas));

app.MapDelete("/api/matriculas/{id}", (int id) =>
{
    var matricula = matriculas.FirstOrDefault(m => m.Id == id);

    if (matricula is null)
        return Error(404, $"No existe ninguna matrícula con el id {id}.");

    matricula.Estado = "Anulada";

    var estudiante = estudiantes.FirstOrDefault(e => e.Id == matricula.EstudianteId);
    if (estudiante is not null)
    {
        estudiante.Matriculado = false;
        estudiante.Tutor = null;
    }

    return Results.NoContent();
});

// ============================================================
// 404 - Cualquier ruta que no exista
// ============================================================
app.MapFallback((HttpContext context) =>
    Error(404, $"La ruta '{context.Request.Path}' no existe. La documentación está en '/'."));

app.Run();