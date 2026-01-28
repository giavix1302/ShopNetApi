using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNetApi.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPaymentFieldsToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create enum types first (only if they don't exist)
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'payment_method') THEN
                        CREATE TYPE payment_method AS ENUM ('cod', 'momo', 'bank');
                    END IF;
                END$$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'payment_status') THEN
                        CREATE TYPE payment_status AS ENUM ('pending', 'paid', 'failed', 'refunded');
                    END IF;
                END$$;
                """);

            // Convert PaymentStatus: map string values to enum integers
            // PENDING = 0, PAID = 1, FAILED = 2, REFUNDED = 3
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders""
                ALTER COLUMN ""PaymentStatus"" TYPE integer
                USING CASE
                    WHEN UPPER(COALESCE(""PaymentStatus"", '')) = 'PENDING' THEN 0
                    WHEN UPPER(COALESCE(""PaymentStatus"", '')) = 'PAID' THEN 1
                    WHEN UPPER(COALESCE(""PaymentStatus"", '')) = 'FAILED' THEN 2
                    WHEN UPPER(COALESCE(""PaymentStatus"", '')) = 'REFUNDED' THEN 3
                    ELSE 0  -- Default to PENDING if value is NULL or invalid
                END;
            ");

            // Set NOT NULL and default value for PaymentStatus
            migrationBuilder.Sql(@"
                UPDATE ""Orders"" SET ""PaymentStatus"" = 0 WHERE ""PaymentStatus"" IS NULL;
                ALTER TABLE ""Orders"" ALTER COLUMN ""PaymentStatus"" SET NOT NULL;
                ALTER TABLE ""Orders"" ALTER COLUMN ""PaymentStatus"" SET DEFAULT 0;
            ");

            // Convert PaymentMethod: map string values to enum integers
            // COD = 0, MOMO = 1, BANK = 2
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders""
                ALTER COLUMN ""PaymentMethod"" TYPE integer
                USING CASE
                    WHEN UPPER(COALESCE(""PaymentMethod"", '')) = 'COD' THEN 0
                    WHEN UPPER(COALESCE(""PaymentMethod"", '')) = 'MOMO' THEN 1
                    WHEN UPPER(COALESCE(""PaymentMethod"", '')) = 'BANK' THEN 2
                    ELSE NULL  -- Keep NULL if value is invalid
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert PaymentStatus back to string
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders""
                ALTER COLUMN ""PaymentStatus"" TYPE character varying(50)
                USING CASE
                    WHEN ""PaymentStatus"" = 0 THEN 'PENDING'
                    WHEN ""PaymentStatus"" = 1 THEN 'PAID'
                    WHEN ""PaymentStatus"" = 2 THEN 'FAILED'
                    WHEN ""PaymentStatus"" = 3 THEN 'REFUNDED'
                    ELSE 'PENDING'
                END;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders"" ALTER COLUMN ""PaymentStatus"" DROP NOT NULL;
                ALTER TABLE ""Orders"" ALTER COLUMN ""PaymentStatus"" DROP DEFAULT;
            ");

            // Convert PaymentMethod back to string
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders""
                ALTER COLUMN ""PaymentMethod"" TYPE character varying(50)
                USING CASE
                    WHEN ""PaymentMethod"" = 0 THEN 'COD'
                    WHEN ""PaymentMethod"" = 1 THEN 'MOMO'
                    WHEN ""PaymentMethod"" = 2 THEN 'BANK'
                    ELSE NULL
                END;
            ");

            // Drop enum types
            migrationBuilder.Sql("DROP TYPE IF EXISTS payment_method;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS payment_status;");
        }
    }
}
