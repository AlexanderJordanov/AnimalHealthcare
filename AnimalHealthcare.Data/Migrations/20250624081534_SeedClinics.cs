using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AnimalHealthcare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedClinics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AnimalClinics",
                columns: new[] { "Id", "Address", "ImageUrl", "Name", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "123 Main St, Springfield", "/images/clinics/happy-paws.jpg", "Happy Paws Veterinary Clinic", "555-1234" },
                    { 2, "456 Elm St, Shelbyville", "/images/clinics/healthy-tails-animal-hospital.jpg", "Healthy Tails Animal Hospital", "555-5678" },
                    { 3, "789 Oak Ave, Capital City", "/images/clinics/gentle-care-pet-clinic.jpg", "Gentle Care Pet Clinic", "555-9012" },
                    { 4, "321 Maple Rd, Ogdenville", "/images/clinics/purrfect-health-vet-center.jpg", "Purrfect Health Vet Center", "555-3456" },
                    { 5, "654 Pine St, North Haverbrook", "/images/clinics/four-legged-friends-vet.jpeg", "Four-Legged Friends Vet", "555-7890" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AnimalClinics",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AnimalClinics",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AnimalClinics",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AnimalClinics",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AnimalClinics",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
