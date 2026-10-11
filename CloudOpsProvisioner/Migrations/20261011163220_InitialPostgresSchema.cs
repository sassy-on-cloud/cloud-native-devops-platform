using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudOpsProvisioner.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProvisioningRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Environment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AwsRegion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeploymentPlatform = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisioningRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningRequests_RequestedAt",
                table: "ProvisioningRequests",
                column: "RequestedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvisioningRequests");
        }
    }
}
