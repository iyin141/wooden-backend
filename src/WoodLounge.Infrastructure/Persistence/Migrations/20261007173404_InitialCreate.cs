using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WoodLounge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateSequence(
                name: "order_number_seq",
                startValue: 10001L);

            migrationBuilder.CreateSequence(
                name: "request_ref_seq",
                startValue: 30001L);

            migrationBuilder.CreateSequence(
                name: "visit_ref_seq",
                startValue: 20001L);

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    slug = table.Column<string>(type: "citext", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "delivery_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    state = table.Column<string>(type: "text", nullable: false),
                    fee_kobo = table.Column<long>(type: "bigint", nullable: false),
                    eta_min_days = table.Column<int>(type: "integer", nullable: false),
                    eta_max_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery_rates", x => x.id);
                    table.CheckConstraint("ck_delivery_rates_fee", "fee_kobo >= 0");
                });

            migrationBuilder.CreateTable(
                name: "otp_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    phone = table.Column<string>(type: "text", nullable: false),
                    code_hash = table.Column<string>(type: "text", nullable: false),
                    purpose = table.Column<string>(type: "text", nullable: false, defaultValue: "login"),
                    attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_otp_codes", x => x.id);
                    table.CheckConstraint("ck_otp_purpose", "purpose in ('login','verify_phone')");
                });

            migrationBuilder.CreateTable(
                name: "payment_webhook_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider = table.Column<string>(type: "text", nullable: false, defaultValue: "paystack"),
                    event_id = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    signature_valid = table.Column<bool>(type: "boolean", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_webhook_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    slug = table.Column<string>(type: "citext", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "showrooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    slug = table.Column<string>(type: "citext", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: true),
                    whatsapp = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    opening_hours_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_showrooms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    phone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "citext", nullable: true),
                    full_name = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "text", nullable: false, defaultValue: "customer"),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    google_sub = table.Column<string>(type: "text", nullable: true),
                    phone_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_phone_or_email", "phone is not null or email is not null");
                    table.CheckConstraint("ck_users_role", "role in ('customer','admin','super_admin')");
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    slug = table.Column<string>(type: "citext", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    short_description = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    base_price_kobo = table.Column<long>(type: "bigint", nullable: false),
                    compare_at_price_kobo = table.Column<long>(type: "bigint", nullable: true),
                    material = table.Column<string>(type: "text", nullable: true),
                    dimensions_text = table.Column<string>(type: "text", nullable: true),
                    width_cm = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    depth_cm = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    height_cm = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    is_made_to_order = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    lead_time_days = table.Column<int>(type: "integer", nullable: true),
                    stock_qty = table.Column<int>(type: "integer", nullable: true),
                    is_bestseller = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_placeholder = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    badge = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "draft"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.CheckConstraint("ck_products_base_price", "base_price_kobo >= 0");
                    table.CheckConstraint("ck_products_compare_price", "compare_at_price_kobo is null or compare_at_price_kobo >= 0");
                    table.CheckConstraint("ck_products_status", "status in ('draft','active','archived')");
                    table.CheckConstraint("ck_products_stock", "stock_qty is null or stock_qty >= 0");
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "visit_availability",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    visit_type = table.Column<string>(type: "text", nullable: false),
                    showroom_id = table.Column<Guid>(type: "uuid", nullable: true),
                    weekday = table.Column<short>(type: "smallint", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    slot_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visit_availability", x => x.id);
                    table.CheckConstraint("ck_visit_availability_time", "end_time > start_time");
                    table.CheckConstraint("ck_visit_availability_type", "visit_type in ('site_inspection','online_visit','showroom_visit')");
                    table.CheckConstraint("ck_visit_availability_weekday", "weekday between 0 and 6");
                    table.ForeignKey(
                        name: "fk_visit_availability_showrooms_showroom_id",
                        column: x => x.showroom_id,
                        principalTable: "showrooms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "visit_blackouts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    on_date = table.Column<DateOnly>(type: "date", nullable: false),
                    visit_type = table.Column<string>(type: "text", nullable: true),
                    showroom_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visit_blackouts", x => x.id);
                    table.CheckConstraint("ck_visit_blackouts_type", "visit_type in ('site_inspection','online_visit','showroom_visit')");
                    table.ForeignKey(
                        name: "fk_visit_blackouts_showrooms_showroom_id",
                        column: x => x.showroom_id,
                        principalTable: "showrooms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: true),
                    recipient = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    line1 = table.Column<string>(type: "text", nullable: false),
                    line2 = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    landmark = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_addresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<string>(type: "text", nullable: false),
                    before = table.Column<string>(type: "jsonb", nullable: true),
                    after = table.Column<string>(type: "jsonb", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_log_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    channel = table.Column<string>(type: "text", nullable: false),
                    to_address = table.Column<string>(type: "text", nullable: false),
                    template = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "queued"),
                    attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.CheckConstraint("ck_notifications_channel", "channel in ('email','whatsapp','sms')");
                    table.CheckConstraint("ck_notifications_status", "status in ('queued','sent','failed')");
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_refresh_tokens_replaced_by_id",
                        column: x => x.replaced_by_id,
                        principalTable: "refresh_tokens",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "site_settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_settings", x => x.key);
                    table.ForeignKey(
                        name: "fk_site_settings_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "site_visit_bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reference = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visit_type = table.Column<string>(type: "text", nullable: false),
                    showroom_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_date = table.Column<DateOnly>(type: "date", nullable: false),
                    requested_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    contact_name = table.Column<string>(type: "text", nullable: false),
                    contact_phone = table.Column<string>(type: "text", nullable: false),
                    contact_email = table.Column<string>(type: "citext", nullable: true),
                    site_address = table.Column<string>(type: "text", nullable: true),
                    site_city = table.Column<string>(type: "text", nullable: true),
                    site_state = table.Column<string>(type: "text", nullable: true),
                    project_notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    proposed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    proposed_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    confirmed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    confirmed_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    meeting_link = table.Column<string>(type: "text", nullable: true),
                    decline_reason = table.Column<string>(type: "text", nullable: true),
                    handled_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_visit_bookings", x => x.id);
                    table.CheckConstraint("ck_visits_showroom", "visit_type <> 'showroom_visit' or showroom_id is not null");
                    table.CheckConstraint("ck_visits_site_address", "visit_type <> 'site_inspection' or site_address is not null");
                    table.CheckConstraint("ck_visits_status", "status in ('pending','confirmed','reschedule_proposed','declined','completed','cancelled')");
                    table.CheckConstraint("ck_visits_type", "visit_type in ('site_inspection','online_visit','showroom_visit')");
                    table.ForeignKey(
                        name: "fk_site_visit_bookings_showrooms_showroom_id",
                        column: x => x.showroom_id,
                        principalTable: "showrooms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_site_visit_bookings_users_handled_by_id",
                        column: x => x.handled_by_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_site_visit_bookings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "custom_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reference = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier = table.Column<string>(type: "text", nullable: false),
                    base_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    room_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    dimensions_text = table.Column<string>(type: "text", nullable: true),
                    preferred_materials = table.Column<string>(type: "text", nullable: true),
                    budget_min_kobo = table.Column<long>(type: "bigint", nullable: true),
                    budget_max_kobo = table.Column<long>(type: "bigint", nullable: true),
                    target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    delivery_city = table.Column<string>(type: "text", nullable: true),
                    delivery_state = table.Column<string>(type: "text", nullable: true),
                    contact_name = table.Column<string>(type: "text", nullable: false),
                    contact_phone = table.Column<string>(type: "text", nullable: false),
                    contact_email = table.Column<string>(type: "citext", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "submitted"),
                    assigned_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_requests", x => x.id);
                    table.CheckConstraint("ck_custom_requests_budget_max", "budget_max_kobo is null or budget_max_kobo >= 0");
                    table.CheckConstraint("ck_custom_requests_budget_min", "budget_min_kobo is null or budget_min_kobo >= 0");
                    table.CheckConstraint("ck_custom_requests_status", "status in ('submitted','in_discussion','quoted','quote_accepted','quote_declined','closed','cancelled')");
                    table.CheckConstraint("ck_custom_requests_tier", "tier in ('modify_existing','fully_bespoke','full_room')");
                    table.ForeignKey(
                        name: "fk_custom_requests_products_base_product_id",
                        column: x => x.base_product_id,
                        principalTable: "products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_custom_requests_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_custom_requests_users_assigned_admin_id",
                        column: x => x.assigned_admin_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_custom_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    alt = table.Column<string>(type: "text", nullable: true),
                    kind = table.Column<string>(type: "text", nullable: false, defaultValue: "studio"),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_images", x => x.id);
                    table.CheckConstraint("ck_product_images_kind", "kind in ('studio','lifestyle','customer_home')");
                    table.ForeignKey(
                        name: "fk_product_images_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_option_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    step_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    input_type = table.Column<string>(type: "text", nullable: false, defaultValue: "select"),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_option_groups", x => x.id);
                    table.CheckConstraint("ck_option_groups_input_type", "input_type in ('select','swatch','radio')");
                    table.ForeignKey(
                        name: "fk_product_option_groups_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_rooms",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_rooms", x => new { x.product_id, x.room_id });
                    table.ForeignKey(
                        name: "fk_product_rooms_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_rooms_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_request_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_key = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_request_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_request_attachments_custom_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "custom_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_threads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    kind = table.Column<string>(type: "text", nullable: false),
                    custom_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    site_visit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_message_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    customer_unread_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    admin_unread_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_threads", x => x.id);
                    table.CheckConstraint("ck_threads_kind", "kind in ('custom_request','site_visit')");
                    table.CheckConstraint("ck_threads_kind_target", "(kind = 'custom_request' and custom_request_id is not null and site_visit_id is null) or (kind = 'site_visit' and site_visit_id is not null and custom_request_id is null)");
                    table.ForeignKey(
                        name: "fk_message_threads_custom_requests_custom_request_id",
                        column: x => x.custom_request_id,
                        principalTable: "custom_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_message_threads_site_visit_bookings_site_visit_id",
                        column: x => x.site_visit_id,
                        principalTable: "site_visit_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_message_threads_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    custom_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    amount_kobo = table.Column<long>(type: "bigint", nullable: false),
                    deposit_kobo = table.Column<long>(type: "bigint", nullable: false),
                    timeline_weeks = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "sent"),
                    sent_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotes", x => x.id);
                    table.CheckConstraint("ck_quotes_amount", "amount_kobo > 0");
                    table.CheckConstraint("ck_quotes_deposit", "deposit_kobo > 0 and deposit_kobo <= amount_kobo");
                    table.CheckConstraint("ck_quotes_status", "status in ('sent','accepted','declined','expired','superseded')");
                    table.CheckConstraint("ck_quotes_timeline", "timeline_weeks > 0");
                    table.ForeignKey(
                        name: "fk_quotes_custom_requests_custom_request_id",
                        column: x => x.custom_request_id,
                        principalTable: "custom_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_quotes_users_sent_by_id",
                        column: x => x.sent_by_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "product_option_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    swatch_hex = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    price_delta_kobo = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_option_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_option_values_product_option_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "product_option_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sender_role = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false, defaultValue: "text"),
                    body = table.Column<string>(type: "text", nullable: false),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.CheckConstraint("ck_messages_kind", "kind in ('text','quote','status_change','system')");
                    table.CheckConstraint("ck_messages_sender_role", "sender_role in ('customer','admin','system')");
                    table.ForeignKey(
                        name: "fk_messages_message_threads_thread_id",
                        column: x => x.thread_id,
                        principalTable: "message_threads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_messages_users_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_number = table.Column<string>(type: "text", nullable: false, defaultValueSql: "('WL-' || nextval('order_number_seq'))"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    guest_email = table.Column<string>(type: "citext", nullable: true),
                    guest_phone = table.Column<string>(type: "text", nullable: true),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    custom_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending_payment"),
                    subtotal_kobo = table.Column<long>(type: "bigint", nullable: false),
                    delivery_fee_kobo = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    discount_kobo = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    total_kobo = table.Column<long>(type: "bigint", nullable: false),
                    amount_paid_kobo = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    balance_due_kobo = table.Column<long>(type: "bigint", nullable: false, computedColumnSql: "total_kobo - amount_paid_kobo", stored: true),
                    ship_name = table.Column<string>(type: "text", nullable: false),
                    ship_phone = table.Column<string>(type: "text", nullable: false),
                    ship_line1 = table.Column<string>(type: "text", nullable: false),
                    ship_line2 = table.Column<string>(type: "text", nullable: true),
                    ship_city = table.Column<string>(type: "text", nullable: false),
                    ship_state = table.Column<string>(type: "text", nullable: false),
                    ship_notes = table.Column<string>(type: "text", nullable: true),
                    estimated_delivery_date = table.Column<DateOnly>(type: "date", nullable: true),
                    admin_note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.CheckConstraint("ck_orders_amount_paid", "amount_paid_kobo >= 0");
                    table.CheckConstraint("ck_orders_contact", "user_id is not null or guest_email is not null or guest_phone is not null");
                    table.CheckConstraint("ck_orders_custom_refs", "kind <> 'custom' or (custom_request_id is not null and quote_id is not null)");
                    table.CheckConstraint("ck_orders_delivery_fee", "delivery_fee_kobo >= 0");
                    table.CheckConstraint("ck_orders_discount", "discount_kobo >= 0");
                    table.CheckConstraint("ck_orders_kind", "kind in ('premade','custom')");
                    table.CheckConstraint("ck_orders_status", "status in ('pending_payment','paid','processing','ready','out_for_delivery','delivered','quote_accepted','deposit_paid','in_production','cancelled','refunded')");
                    table.CheckConstraint("ck_orders_subtotal", "subtotal_kobo >= 0");
                    table.CheckConstraint("ck_orders_total", "total_kobo >= 0");
                    table.ForeignKey(
                        name: "fk_orders_custom_requests_custom_request_id",
                        column: x => x.custom_request_id,
                        principalTable: "custom_requests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "message_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_key = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_message_attachments_messages_message_id",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name_snapshot = table.Column<string>(type: "text", nullable: false),
                    sku_snapshot = table.Column<string>(type: "text", nullable: true),
                    image_url_snapshot = table.Column<string>(type: "text", nullable: true),
                    unit_price_kobo = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    selected_options = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    line_total_kobo = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.CheckConstraint("ck_order_items_line_total", "line_total_kobo >= 0");
                    table.CheckConstraint("ck_order_items_qty", "quantity > 0");
                    table.CheckConstraint("ck_order_items_unit_price", "unit_price_kobo >= 0");
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "order_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    notify_customer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    changed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_status_history_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_status_history_users_changed_by_id",
                        column: x => x.changed_by_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    provider = table.Column<string>(type: "text", nullable: false, defaultValue: "paystack"),
                    reference = table.Column<string>(type: "text", nullable: false),
                    paystack_transaction_id = table.Column<long>(type: "bigint", nullable: true),
                    amount_kobo = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false, defaultValue: "NGN"),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "initialized"),
                    channel = table.Column<string>(type: "text", nullable: true),
                    fees_kobo = table.Column<long>(type: "bigint", nullable: true),
                    customer_email = table.Column<string>(type: "citext", nullable: false),
                    authorization_url = table.Column<string>(type: "text", nullable: true),
                    access_code = table.Column<string>(type: "text", nullable: true),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    raw = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.CheckConstraint("ck_payments_amount", "amount_kobo > 0");
                    table.CheckConstraint("ck_payments_kind", "kind in ('full','deposit','balance')");
                    table.CheckConstraint("ck_payments_status", "status in ('initialized','pending','success','failed','abandoned','refunded')");
                    table.ForeignKey(
                        name: "fk_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_addresses_user",
                table: "addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entity",
                table: "audit_log",
                columns: new[] { "entity_type", "entity_id", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_actor_user_id",
                table: "audit_log",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_id",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_custom_request_attachments_request_id",
                table: "custom_request_attachments",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_requests_assigned_admin_id",
                table: "custom_requests",
                column: "assigned_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_requests_base_product_id",
                table: "custom_requests",
                column: "base_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_requests_reference",
                table: "custom_requests",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_custom_requests_room_id",
                table: "custom_requests",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_requests_status",
                table: "custom_requests",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_custom_requests_user",
                table: "custom_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_rates_state",
                table: "delivery_rates",
                column: "state",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_message_attachments_message_id",
                table: "message_attachments",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_threads_user_id",
                table: "message_threads",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_thread_custom",
                table: "message_threads",
                column: "custom_request_id",
                unique: true,
                filter: "custom_request_id is not null");

            migrationBuilder.CreateIndex(
                name: "ux_thread_visit",
                table: "message_threads",
                column: "site_visit_id",
                unique: true,
                filter: "site_visit_id is not null");

            migrationBuilder.CreateIndex(
                name: "ix_messages_quote_id",
                table: "messages",
                column: "quote_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_sender_user_id",
                table: "messages",
                column: "sender_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_thread",
                table: "messages",
                columns: new[] { "thread_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_queue",
                table: "notifications",
                columns: new[] { "status", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_history_order",
                table: "order_status_history",
                columns: new[] { "order_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_order_status_history_changed_by_id",
                table: "order_status_history",
                column: "changed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_custom_request_id",
                table: "orders",
                column: "custom_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_guest_email",
                table: "orders",
                column: "guest_email");

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_quote_id",
                table: "orders",
                column: "quote_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_created",
                table: "orders",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_orders_user",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_otp_phone_created",
                table: "otp_codes",
                columns: new[] { "phone", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_payment_webhook_events_provider_event_id",
                table: "payment_webhook_events",
                columns: new[] { "provider", "event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_order",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_reference",
                table: "payments",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_status_created",
                table: "payments",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_product_images_product",
                table: "product_images",
                columns: new[] { "product_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_option_groups_product",
                table: "product_option_groups",
                columns: new[] { "product_id", "step_order" });

            migrationBuilder.CreateIndex(
                name: "ix_option_values_group",
                table: "product_option_values",
                columns: new[] { "group_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_product_rooms_room_id",
                table: "product_rooms",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_category",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_name_trgm",
                table: "products",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_slug",
                table: "products",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_status",
                table: "products",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_custom_request_id_version",
                table: "quotes",
                columns: new[] { "custom_request_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quotes_sent_by_id",
                table: "quotes",
                column: "sent_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_replaced_by_id",
                table: "refresh_tokens",
                column: "replaced_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_user",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_slug",
                table: "rooms",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_showrooms_slug",
                table: "showrooms",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_site_settings_updated_by_id",
                table: "site_settings",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_visit_bookings_handled_by_id",
                table: "site_visit_bookings",
                column: "handled_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_visit_bookings_reference",
                table: "site_visit_bookings",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_site_visit_bookings_showroom_id",
                table: "site_visit_bookings",
                column: "showroom_id");

            migrationBuilder.CreateIndex(
                name: "ix_visits_status_date",
                table: "site_visit_bookings",
                columns: new[] { "status", "requested_date" });

            migrationBuilder.CreateIndex(
                name: "ix_visits_user",
                table: "site_visit_bookings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_google_sub",
                table: "users",
                column: "google_sub",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_phone",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_visit_availability_showroom_id",
                table: "visit_availability",
                column: "showroom_id");

            migrationBuilder.CreateIndex(
                name: "ix_visit_blackouts_showroom_id",
                table: "visit_blackouts",
                column: "showroom_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "custom_request_attachments");

            migrationBuilder.DropTable(
                name: "delivery_rates");

            migrationBuilder.DropTable(
                name: "message_attachments");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "order_status_history");

            migrationBuilder.DropTable(
                name: "otp_codes");

            migrationBuilder.DropTable(
                name: "payment_webhook_events");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "product_images");

            migrationBuilder.DropTable(
                name: "product_option_values");

            migrationBuilder.DropTable(
                name: "product_rooms");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "site_settings");

            migrationBuilder.DropTable(
                name: "visit_availability");

            migrationBuilder.DropTable(
                name: "visit_blackouts");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "product_option_groups");

            migrationBuilder.DropTable(
                name: "message_threads");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropTable(
                name: "site_visit_bookings");

            migrationBuilder.DropTable(
                name: "custom_requests");

            migrationBuilder.DropTable(
                name: "showrooms");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropSequence(
                name: "order_number_seq");

            migrationBuilder.DropSequence(
                name: "request_ref_seq");

            migrationBuilder.DropSequence(
                name: "visit_ref_seq");
        }
    }
}
