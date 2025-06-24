using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalHealthcare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUserAndProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2", 0, "208685d2-107b-443f-ba5b-88c86d4771b1", "admin@animalhealthcare.com", true, false, null, "ADMIN@ANIMALHEALTHCARE.COM", "ADMIN@ANIMALHEALTHCARE.COM", "AQAAAAIAAYagAAAAEPbX9OvboHXNH3Jf26Zr+QyoankS5Ew8mnxiJnXpvAIkq+bB6JN+ZkT9cxbVgHJthQ==", null, false, "62e0e177-f8c0-40b7-a30a-47df677653a9", false, "admin@animalhealthcare.com" });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "Id", "Address", "FullName", "PhoneNumber", "ProfilePictureUrl" },
                values: new object[] { "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2", "123 Admin St, Admin City, Admin Country", "Admin User", "123-456-7890", "/images/profiles/admin.jpg" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "28d4fc17-fdaf-4dc5-9bb3-5cc0b4c64bc2");
        }
    }
}
