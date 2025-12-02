using MauiApp7.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MauiApp7.Data
{
    public static class DatabaseConnection
    {
        private static readonly string ConnectionString =
            @"Server=(localdb)\mssqllocaldb;Database=Escuela;Trusted_Connection=true;TrustServerCertificate=true;";

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);

        // --------------------------------------------------
        // CREATE: Inserta un nuevo alumno
        // --------------------------------------------------
        public static async Task<bool> InsertarAlumnoAsync(Alumno alumno)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            if (await ExisteAlumno(connection, alumno.CURP, alumno.Matricula))
                return false;

            using var cmd = new SqlCommand(@"
        INSERT INTO Alumnos (Nombre, ApePat, ApeMat, CURP, Matricula, Telefono)
        VALUES (@Nombre, @ApePat, @ApeMat, @CURP, @Matricula, @Telefono)", connection);

            cmd.Parameters.AddWithValue("@Nombre", alumno.Nombre);
            cmd.Parameters.AddWithValue("@ApePat", alumno.ApePat);
            cmd.Parameters.AddWithValue("@ApeMat", alumno.ApeMat);
            cmd.Parameters.AddWithValue("@CURP", alumno.CURP);
            cmd.Parameters.AddWithValue("@Matricula", alumno.Matricula);
            cmd.Parameters.AddWithValue("@Telefono", alumno.Telefono);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }

        // --------------------------------------------------
        // READ: Obtiene todos los alumnos
        // --------------------------------------------------
        public static async Task<List<Alumno>> ObtenerAlumnosAsync()
        {
            var alumnos = new List<Alumno>();
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id_Alumno, Nombre, ApePat, ApeMat, CURP, Matricula, Telefono 
                FROM Alumnos", connection);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alumnos.Add(new Alumno
                {
                    Id_Alumno = reader.GetInt32("Id_Alumno"),
                    Nombre = reader.GetString("Nombre"),
                    ApePat = reader.GetString("ApePat"),
                    ApeMat = reader.GetString("ApeMat"),
                    CURP = reader.GetString("CURP"),
                    Matricula = reader.GetString("Matricula"),
                    Telefono = reader.GetString("Telefono")
                });
            }

            return alumnos;
        }

        // --------------------------------------------------
        // UPDATE: Actualiza un alumno existente
        // --------------------------------------------------
        public static async Task<bool> ActualizarAlumnoAsync(Alumno alumno)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Verificar que no haya conflicto con otro registro (excepto el actual)
            if (await ExisteAlumnoExcluyendoId(connection, alumno.Id_Alumno, alumno.CURP, alumno.Matricula))
                return false;

            using var cmd = new SqlCommand(@"
                UPDATE Alumnos 
                SET Nombre = @Nombre, 
                    ApePat = @ApePat, 
                    ApeMat = @ApeMat, 
                    CURP = @CURP, 
                    Matricula = @Matricula, 
                    Telefono = @Telefono
                WHERE Id_Alumno = @Id", connection);

            cmd.Parameters.AddWithValue("@Id", alumno.Id_Alumno);
            cmd.Parameters.AddWithValue("@Nombre", alumno.Nombre);
            cmd.Parameters.AddWithValue("@ApePat", alumno.ApePat);
            cmd.Parameters.AddWithValue("@ApeMat", alumno.ApeMat);
            cmd.Parameters.AddWithValue("@CURP", alumno.CURP);
            cmd.Parameters.AddWithValue("@Matricula", alumno.Matricula);
            cmd.Parameters.AddWithValue("@Telefono", alumno.Telefono);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // --------------------------------------------------
        // DELETE: Elimina un alumno por ID
        // --------------------------------------------------
        public static async Task<bool> EliminarAlumnoAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var cmd = new SqlCommand("DELETE FROM Alumnos WHERE Id_Alumno = @Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // --------------------------------------------------
        // MÉTODOS AUXILIARES
        // --------------------------------------------------
        private static async Task<bool> ExisteAlumno(SqlConnection connection, string curp, string matricula)
        {
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Alumnos 
                WHERE CURP = @CURP OR Matricula = @Matricula", connection);

            cmd.Parameters.AddWithValue("@CURP", curp);
            cmd.Parameters.AddWithValue("@Matricula", matricula);

            var result = await cmd.ExecuteScalarAsync();
            return (int)(result ?? 0) > 0;
        }

        private static async Task<bool> ExisteAlumnoExcluyendoId(SqlConnection connection, int id, string curp, string matricula)
        {
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Alumnos 
                WHERE (CURP = @CURP OR Matricula = @Matricula)
                  AND Id_Alumno != @Id", connection);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@CURP", curp);
            cmd.Parameters.AddWithValue("@Matricula", matricula);

            var result = await cmd.ExecuteScalarAsync();
            return (int)(result ?? 0) > 0;
        }
    }
}