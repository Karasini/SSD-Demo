using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transcriptor.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeakerDiarizationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpeakerLabelsJson",
                table: "TranscriptionJobs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscriptSegmentsJson",
                table: "TranscriptionJobs",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpeakerLabelsJson",
                table: "TranscriptionJobs");

            migrationBuilder.DropColumn(
                name: "TranscriptSegmentsJson",
                table: "TranscriptionJobs");
        }
    }
}
