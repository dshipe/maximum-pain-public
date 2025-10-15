using MaxPainInfrastructure.Code;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MaxPainInfrastructure.Models
{
    public class AwsContext : DbContext
    {
        public AwsContext() : base()
        {
        }

        public AwsContext(DbContextOptions<AwsContext> options) : base(options)
        {
        }

        public DbSet<EmailAccount> EmailAccount { get; set; }
        public DbSet<EmailStat> EmailStat { get; set; }
        public DbSet<Message> Message { get; set; }

        public DbSet<Hop> Hop { get; set; }
        public DbSet<OptionChainJson> OptionChainJson { get; set; }
        public DbSet<TwitterXml> TwitterXml { get; set; }
        public DbSet<StockTicker> StockTicker { get; set; }
        public DbSet<BlogEntry> BlogEntry { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailStat>().HasNoKey().ToView("vwEmailStat")
                .Property(v => v.Date).HasColumnName("Date");

            var twitterXmlEntity = modelBuilder.Entity<TwitterXml>();
            foreach (var property in twitterXmlEntity.Metadata.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    twitterXmlEntity.Property<DateTime>(property.Name)
                        .HasConversion(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    twitterXmlEntity.Property<DateTime?>(property.Name)
                        .HasConversion(
                            v => v.HasValue ? v.Value.ToUniversalTime() : v,
                            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> Execute(string sql, List<SqlParameter>? parms, int timeout)
        {
            return await DBHelper.Execute(this.Database, sql, parms, timeout);
        }

        public async Task<string> FetchJson(string sql, List<SqlParameter>? parms, int timeout)
        {
            return await DBHelper.FetchJson(this.Database, sql, parms);
        }

        public async Task<object> FetchScalar(string sql, List<SqlParameter>? parms, string fieldName)
        {
            return await DBHelper.FetchScalar(this.Database, sql, parms, fieldName);
        }

        public async Task<string> SettingsRead()
        {
            string sql = "SELECT Id, Content, ModifiedOn FROM SettingsXml WITH(NOLOCK)";
            List<SqlParameter> parms = new List<SqlParameter>();

            string content = await DBHelper.FetchContent(this.Database, sql, parms);
            return content;
        }

        public async Task<bool> SettingsPost(string xml)
        {
            string sql = "UPDATE SettingsXML SET Content=@Content";
            List<SqlParameter> parms = new List<SqlParameter>
            {
                new SqlParameter("Content", xml)
            };
            await DBHelper.Execute(this.Database, sql, parms);
            return true;
        }

        public async Task<List<PythonTicker>?> GetPythonTicker()
        {
            string sql = @"
                SELECT Ticker, Source, CreatedOn
                FROM Python..Ticker WITH(NOLOCK)
                WHERE Source='sp500' 
                ORDER BY Ticker
            ";

            string json = await DBHelper.FetchJson(this.Database, sql, null);
            List<PythonTicker>? tickers = DBHelper.Deserialize<List<PythonTicker>>(json);
            if (tickers != null)
            {
                tickers.AddRange(new[]
                {
                    new PythonTicker { Ticker = "SPX", Source = "sp500", CreatedOn = DateTime.UtcNow },
                    new PythonTicker { Ticker = "QQQ", Source = "sp500", CreatedOn = DateTime.UtcNow }
                });
            }
            return tickers;
        }
    }
}
