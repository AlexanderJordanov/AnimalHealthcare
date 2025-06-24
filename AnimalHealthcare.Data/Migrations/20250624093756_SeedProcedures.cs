using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AnimalHealthcare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "22222222-2222-2222-2222-222222222222", "AQAAAAIAAYagAAAAEBmKoup3TJhw47bvcqlUwPabiIwPFZOLI7qc46/vZm2L+gLbHeatMyc2TEcT+t/Hjw==", "11111111-1111-1111-1111-111111111111" });

            migrationBuilder.InsertData(
                table: "Procedures",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Routine animal vaccination", "Vaccination" },
                    { 2, "Professional dental cleaning for pets", "Dental Cleaning" },
                    { 3, "Spaying or neutering surgery", "Spay/Neuter" },
                    { 4, "Microchip implantation for ID", "Microchipping" },
                    { 5, "Standard diagnostic X-ray", "X-Ray" },
                    { 6, "Routine blood analysis", "Blood Test" },
                    { 7, "Abdominal ultrasound exam", "Ultrasound" },
                    { 8, "Testing for heartworm disease", "Heartworm Test" },
                    { 9, "Skin or blood allergy tests", "Allergy Testing" },
                    { 10, "Surgery for bone/joint issues", "Orthopedic Surgery" },
                    { 11, "Collection and analysis of skin sample", "Skin Biopsy" },
                    { 12, "Comprehensive eye examination", "Eye Exam" },
                    { 13, "Professional ear cleaning", "Ear Cleaning" },
                    { 14, "Treatment for internal/external parasites", "Parasite Treatment" },
                    { 15, "Treatment and bandaging of wounds", "Wound Care" },
                    { 16, "Surgical tumor removal", "Tumor Removal" },
                    { 17, "Consultation for behavioral issues", "Behavioral Consultation" },
                    { 18, "Trimming of nails/claws", "Nail Trim" },
                    { 19, "Basic grooming service", "Grooming" },
                    { 20, "Immediate emergency treatment", "Emergency Care" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Procedures",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "208685d2-107b-443f-ba5b-88c86d4771b1", "AQAAAAIAAYagAAAAEPbX9OvboHXNH3Jf26Zr+QyoankS5Ew8mnxiJnXpvAIkq+bB6JN+ZkT9cxbVgHJthQ==", "62e0e177-f8c0-40b7-a30a-47df677653a9" });
        }
    }
}
