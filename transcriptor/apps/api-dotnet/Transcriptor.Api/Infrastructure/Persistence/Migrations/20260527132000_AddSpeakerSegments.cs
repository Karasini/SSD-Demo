using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transcriptor.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeakerSegments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpeakerAliasesJson",
                table: "TranscriptionJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscriptSegmentsJson",
                table: "TranscriptionJobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpeakerAliasesJson",
                table: "TranscriptionJobs");

            migrationBuilder.DropColumn(
                name: "TranscriptSegmentsJson",
                table: "TranscriptionJobs");
        }
    }
}
