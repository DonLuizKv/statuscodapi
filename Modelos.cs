
public class Estudiante
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int Edad { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string Sexo { get; set; } = "";
    public string Alergias { get; set; } = "Ninguna";
    public double Promedio { get; set; }
    public string EstadoSalud { get; set; } = "Estable";
    public string Grado { get; set; } = "";
    public string Seccion { get; set; } = "";
    public bool Matriculado { get; set; } = true;
    public string? Tutor { get; set; }
}

public class Docente
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int Edad { get; set; }
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "Docente";
}

public class Matricula
{
    public int Id { get; set; }
    public int EstudianteId { get; set; }
    public string CodigoDocente { get; set; } = "";
    public string Grado { get; set; } = "";
    public string Seccion { get; set; } = "";
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = "Pendiente";
}


public class EstudianteDatos
{
    public string? Nombre { get; set; }
    public int? Edad { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? Alergias { get; set; }
    public double? Promedio { get; set; }
    public string? Grado { get; set; }
    public string? Seccion { get; set; }
}

public class DocenteDatos
{
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public int? Edad { get; set; }
    public string? Email { get; set; }
    public string? Rol { get; set; }
}

public class MatriculaDatos
{
    public int? EstudianteId { get; set; }
    public string? CodigoDocente { get; set; }
    public string? Grado { get; set; }
    public string? Seccion { get; set; }
    public DateOnly? Fecha { get; set; }
}

public class Error
{
    public int Estado { get; set; }
    public string Mensaje { get; set; } = "";
    public DateTime Fecha { get; set; } = DateTime.Now;
}