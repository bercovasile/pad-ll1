using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Broker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "LogTopics",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    ConsumerOffsets = table.Column<Dictionary<string, long>>(type: "jsonb", nullable: false),
                    Partitions = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Behavior = table.Column<int>(type: "integer", nullable: false),
                    LastMessageTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalMessagesProduced = table.Column<long>(type: "bigint", nullable: false),
                    TotalMessagesConsumed = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    Offset = table.Column<long>(type: "bigint", nullable: true),
                    PartitionKey = table.Column<string>(type: "text", nullable: true),
                    Acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_LogTopics_TopicId",
                        column: x => x.TopicId,
                        principalSchema: "public",
                        principalTable: "LogTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Offset",
                schema: "public",
                table: "Messages",
                column: "Offset");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Timestamp",
                schema: "public",
                table: "Messages",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TopicId",
                schema: "public",
                table: "Messages",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LogTopics",
                schema: "public");
        }
    }
}
