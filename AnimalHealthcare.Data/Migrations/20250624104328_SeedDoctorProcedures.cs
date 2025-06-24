using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AnimalHealthcare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDoctorProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DoctorProcedures",
                columns: new[] { "DoctorId", "ProcedureId" },
                values: new object[,]
                {
                    { 1, 3 },
                    { 1, 10 },
                    { 1, 16 },
                    { 2, 2 },
                    { 3, 9 },
                    { 3, 11 },
                    { 4, 5 },
                    { 4, 7 },
                    { 5, 6 },
                    { 5, 8 },
                    { 7, 12 },
                    { 8, 10 },
                    { 9, 16 },
                    { 10, 6 },
                    { 11, 15 },
                    { 11, 20 },
                    { 12, 1 },
                    { 12, 4 },
                    { 12, 13 },
                    { 12, 14 },
                    { 13, 1 },
                    { 13, 18 },
                    { 14, 17 },
                    { 14, 19 }
                });

            migrationBuilder.InsertData(
                table: "Procedures",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 21, "Specialized consultation for neurological conditions", "Neurological Consultation" });

            migrationBuilder.InsertData(
                table: "DoctorProcedures",
                columns: new[] { "DoctorId", "ProcedureId" },
                values: new object[] { 6, 21 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 1, 10 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 1, 16 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 3, 9 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 3, 11 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 4, 5 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 4, 7 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 5, 6 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 5, 8 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 6, 21 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 7, 12 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 8, 10 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 9, 16 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 10, 6 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 11, 15 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 11, 20 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 12, 1 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 12, 4 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 12, 13 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 12, 14 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 13, 1 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 13, 18 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 14, 17 });

            migrationBuilder.DeleteData(
                table: "DoctorProcedures",
                keyColumns: new[] { "DoctorId", "ProcedureId" },
                keyValues: new object[] { 14, 19 });

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 21);
        }
    }
}
