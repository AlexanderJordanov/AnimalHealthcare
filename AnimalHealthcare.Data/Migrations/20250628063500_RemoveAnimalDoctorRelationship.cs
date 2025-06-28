using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalHealthcare.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAnimalDoctorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_Doctors_DoctorId",
                table: "Animals");

            migrationBuilder.DropIndex(
                name: "IX_Animals_DoctorId",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Animals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Animals",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Animals",
                keyColumn: "Id",
                keyValue: 1,
                column: "DoctorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Animals",
                keyColumn: "Id",
                keyValue: 2,
                column: "DoctorId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Animals_DoctorId",
                table: "Animals",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_Doctors_DoctorId",
                table: "Animals",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
