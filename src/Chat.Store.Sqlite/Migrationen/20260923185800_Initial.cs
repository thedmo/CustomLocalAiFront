using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Store.Sqlite.Migrationen;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Unterhaltung",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Titel = table.Column<string>(type: "TEXT", nullable: false),
                ErstelltAm = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Unterhaltung", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Nachricht",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                UnterhaltungId = table.Column<Guid>(type: "TEXT", nullable: false),
                Text = table.Column<string>(type: "TEXT", nullable: false),
                Zeit = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Nachricht", x => x.Id);
                table.ForeignKey(
                    name: "FK_Nachricht_Unterhaltung_UnterhaltungId",
                    column: x => x.UnterhaltungId,
                    principalTable: "Unterhaltung",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Antwort",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                NachrichtId = table.Column<Guid>(type: "TEXT", nullable: false),
                Text = table.Column<string>(type: "TEXT", nullable: false),
                Zustand = table.Column<int>(type: "INTEGER", nullable: false),
                DauerMs = table.Column<long>(type: "INTEGER", nullable: true),
                Fall = table.Column<int>(type: "INTEGER", nullable: true),
                Grund = table.Column<string>(type: "TEXT", nullable: true),
                ErstelltAm = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Antwort", x => x.Id);
                table.ForeignKey(
                    name: "FK_Antwort_Nachricht_NachrichtId",
                    column: x => x.NachrichtId,
                    principalTable: "Nachricht",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Antwort_NachrichtId",
            table: "Antwort",
            column: "NachrichtId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Nachricht_UnterhaltungId",
            table: "Nachricht",
            column: "UnterhaltungId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Antwort");

        migrationBuilder.DropTable(
            name: "Nachricht");

        migrationBuilder.DropTable(
            name: "Unterhaltung");
    }
}
