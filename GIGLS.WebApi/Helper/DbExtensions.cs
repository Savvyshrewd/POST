﻿using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace POST.WebApi.Helper
{
    internal static class DbExtensions
    {
        internal static bool IsDatabaseFkDeleteException(this DbUpdateException updateEx, out string foreignKeyErrorMessage)
        {
            foreignKeyErrorMessage = null;

            if (updateEx == null || updateEx.Entries.All(e => e.State != EntityState.Deleted))
                return false;

            var exception = (updateEx.InnerException ?? updateEx.InnerException?.InnerException) as SqlException;
            var errors = exception?.Errors.Cast<SqlError>();

            var errorMessages = new StringBuilder();
            if (errors != null)
            {
                foreach (var exceptionError in errors.Where(e => e.Number == 547))
                {
                    errorMessages.AppendLine($"Message: {exceptionError.Message}");
                    errorMessages.AppendLine($"ErrorNumber: {exceptionError.Number}");
                    errorMessages.AppendLine($"LineNumber: {exceptionError.LineNumber}");
                    errorMessages.AppendLine($"Source: {exceptionError.Source}");
                    errorMessages.AppendLine($"Procedure: {exceptionError.Procedure}");
                }
            }

            if (errorMessages.Length == 0) return false;

            foreignKeyErrorMessage = errorMessages.ToString();

            return true;
        }
    }
}