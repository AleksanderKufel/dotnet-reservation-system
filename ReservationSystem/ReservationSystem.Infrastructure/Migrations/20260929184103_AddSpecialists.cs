using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReservationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialists", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Specialists",
                columns: new[] { "Id", "Name", "Specialization" },
                values: new object[,]
                {
                    { new Guid("3f1c9a52-6b1e-4f5a-9d2e-1a7b8c9d0e01"), "Anna Nowak", "Family physician" },
                    { new Guid("3f1c9a52-6b1e-4f5a-9d2e-1a7b8c9d0e02"), "Piotr Kowalski", "Pediatrician" },
                    { new Guid("3f1c9a52-6b1e-4f5a-9d2e-1a7b8c9d0e03"), "Marta Wiśniewska", "Dermatologist" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SpecialistId",
                table: "Reservations",
                column: "SpecialistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Specialists_SpecialistId",
                table: "Reservations",
                column: "SpecialistId",
                principalTable: "Specialists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Specialists_SpecialistId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "Specialists");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_SpecialistId",
                table: "Reservations");
        }
    }
}
