using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiAggregator.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RelevanceScore = table.Column<double>(type: "double precision", nullable: true),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RetrievedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiPerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AverageResponseTimeMs = table.Column<double>(type: "double precision", nullable: false),
                    TotalRequests = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulRequests = table.Column<int>(type: "integer", nullable: false),
                    FailedRequests = table.Column<int>(type: "integer", nullable: false),
                    SnapshotTakenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiPerformanceSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiRequestLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    UsedFallback = table.Column<bool>(type: "boolean", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRequestLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalApiProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalApiProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiPerformanceSnapshots_ApiName",
                table: "ApiPerformanceSnapshots",
                column: "ApiName");

            migrationBuilder.CreateIndex(
                name: "IX_ApiPerformanceSnapshots_SnapshotTakenAt",
                table: "ApiPerformanceSnapshots",
                column: "SnapshotTakenAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApiRequestLogs_ApiName",
                table: "ApiRequestLogs",
                column: "ApiName");

            migrationBuilder.CreateIndex(
                name: "IX_ApiRequestLogs_RequestedAt",
                table: "ApiRequestLogs",
                column: "RequestedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatedItems");

            migrationBuilder.DropTable(
                name: "ApiPerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "ApiRequestLogs");

            migrationBuilder.DropTable(
                name: "ExternalApiProviders");
        }
    }
}
