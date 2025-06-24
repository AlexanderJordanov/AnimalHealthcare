using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AnimalHealthcare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAnimalsWithNullableDoctorIdAndGenderEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Animals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Animals",
                type: "nvarchar(max)",
                nullable: false
            );

            migrationBuilder.InsertData(
                table: "Animals",
                columns: new[] { "Id", "Age", "Breed", "DoctorId", "Gender", "Name", "Species", "UserProfileId" },
                values: new object[,]
                {
                    { 1, 4, "Labrador Retriever", null, "Male", "Buddy", "Dog", "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2" },
                    { 2, 3, "Siamese", null, "Female", "Whiskers", "Cat", "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Animals",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Animals",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Animals");

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Animals",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
