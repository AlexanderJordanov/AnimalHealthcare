using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AnimalHealthcare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDoctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "AnimalClinicId", "ImageUrl", "Name", "PhoneNumber", "Specialization", "YearsOfExperience" },
                values: new object[,]
                {
                    { 1, 1, "/images/doctors/sarah-smith.jpg", "Dr. Sarah Smith", "555-1001", "Surgery", 10 },
                    { 2, 1, "/images/doctors/james-brown.jpg", "Dr. James Brown", "555-1002", "Dentistry", 8 },
                    { 3, 1, "/images/doctors/laura-green.jpg", "Dr. Laura Green", "555-1003", "Dermatology", 6 },
                    { 4, 1, "/images/doctors/mark-white.jpg", "Dr. Mark White", "555-1004", "Radiology", 9 },
                    { 5, 2, "/images/doctors/olivia-wilson.jpg", "Dr. Olivia Wilson", "555-1005", "Cardiology", 12 },
                    { 6, 2, "/images/doctors/daniel-martinez.jpg", "Dr. Daniel Martinez", "555-1006", "Neurology", 7 },
                    { 7, 2, "/images/doctors/emma-clark.jpg", "Dr. Emma Clark", "555-1007", "Ophthalmology", 5 },
                    { 8, 3, "/images/doctors/michael-lee.jpg", "Dr. Michael Lee", "555-1008", "Orthopedics", 11 },
                    { 9, 3, "/images/doctors/sophia-garcia.jpg", "Dr. Sophia Garcia", "555-1009", "Oncology", 5 },
                    { 10, 3, "/images/doctors/liam-king.jpg", "Dr. Liam King", "555-1010", "Endocrinology", 4 },
                    { 11, 4, "/images/doctors/william-johnson.jpg", "Dr. William Johnson", "555-1011", "Emergency Medicine", 13 },
                    { 12, 4, "/images/doctors/mia-hernandez.jpg", "Dr. Mia Hernandez", "555-1012", "General Practice", 4 },
                    { 13, 5, "/images/doctors/ethan-robinson.jpg", "Dr. Ethan Robinson", "555-1013", "Pediatrics", 8 },
                    { 14, 5, "/images/doctors/isabella-lopez.jpg", "Dr. Isabella Lopez", "555-1014", "Geriatrics", 6 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 14);
        }
    }
}
