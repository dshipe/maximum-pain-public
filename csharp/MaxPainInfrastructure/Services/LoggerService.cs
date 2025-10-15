using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MaxPainInfrastructure.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly AwsContext _awsContext;

        public LoggerService(AwsContext context)
        {
            _awsContext = context;
        }

        public async Task<bool> InfoAsync(string subject, string body)
        {
            const string sql = @"
                INSERT INTO Message (Subject, Body, CreatedOn)
                VALUES (@ParmSubject, @ParmBody, GetUTCDate())
            ";

            if (body.Length > 3000) body = body.Substring(0, 3000);

            var parameters = new List<SqlParameter>();
            parameters.Add(DBHelper.CreateParm("ParmSubject", SqlDbType.VarChar, subject));
            parameters.Add(DBHelper.CreateParm("ParmBody", SqlDbType.Text, DBHelper.Serialize(body)));

            await _awsContext.Execute(sql, parameters, 30);

            return true;
        }

        public async Task<bool> InfoAsyncEF(string subject, string body)
        {
            if (body.Length > 3000) body = body.Substring(0, 3000);

            var msg = new Message
            {
                Subject = subject,
                Body = body,
                CreatedOn = DateTime.UtcNow
            };

            _awsContext.Message.Add(msg);
            await _awsContext.SaveChangesAsync();

            return true;
        }
    }
}