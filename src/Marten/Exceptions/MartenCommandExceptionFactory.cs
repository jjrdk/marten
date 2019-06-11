using System;
using Marten.Services;
using Npgsql;

namespace Marten.Exceptions
{
    /// <summary>
    /// Class responsible for MartenCommandException creation.
    /// Based on the exact command code and exception creates
    /// generi MartenCommandException or specific Exception derived from it.
    /// </summary>
    internal static class MartenCommandExceptionFactory
    {
        internal static MartenCommandException Create
        (
            NpgsqlCommand command,
            Exception innerException
        )
        {
            if (TryToMapToMartenCommandNotSupportedException(command, innerException, out var notSupportedException))
            {
                return notSupportedException;
            }

            return new MartenCommandException(command, innerException);
        }

        internal static bool TryToMapToMartenCommandNotSupportedException
        (
            NpgsqlCommand command,
            Exception innerException,
            out MartenCommandNotSupportedException notSupportedException
        )
        {
            var postgresException = innerException as PostgresException;
            if (postgresException?.SqlState != PostgresErrorCodes.FunctionDoesNotExist || !postgresException.Message.Contains("tsvector"))
            {
                notSupportedException = null;
                return false;
            }

            notSupportedException = new MartenCommandNotSupportedException
            (
                NotSupportedReason.FullTextSearchNeedsAtLeastPostgresVersion10,
                command,
                innerException
            );
            return true;
        }
    }
}
