using Sprout.Core.Services.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Sprout.Tests.Integration.SqlServer.TestCases
{
    internal class SQLServerUserTaskTestCase
    {
        public static async Task Create(SqlServerService sqlSvc)
        {
            var sql =
                """
                -- ============================================================================
                -- Clean Up Existing Tables (Drop in reverse order of foreign key dependencies)
                -- ============================================================================
                IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL 
                    DROP TABLE dbo.Tasks;

                IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL 
                    DROP TABLE dbo.Users;

                IF OBJECT_ID('dbo.UserTypes', 'U') IS NOT NULL 
                    DROP TABLE dbo.UserTypes;

                -- ============================================================================
                -- 1. Create UserTypes Table & Insert Data
                -- ============================================================================
                CREATE TABLE dbo.UserTypes (
                    ID INT IDENTITY(1,1) CONSTRAINT PK_UserTypes PRIMARY KEY,
                    Name NVARCHAR(50) NOT NULL
                );

                INSERT INTO dbo.UserTypes (Name)
                VALUES 
                    ('Administrator'),
                    ('Standard User'),
                    ('Guest');

                -- ============================================================================
                -- 2. Create Users Table & Insert Data
                -- ============================================================================
                CREATE TABLE dbo.Users (
                    ID INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
                    UserName NVARCHAR(100) NOT NULL,
                    Email NVARCHAR(100) NOT NULL,
                    UserTypeID INT NOT NULL,
                    CONSTRAINT FK_Users_UserTypes FOREIGN KEY (UserTypeID) 
                        REFERENCES dbo.UserTypes(ID)
                );

                INSERT INTO dbo.Users (UserName, Email, UserTypeID)
                VALUES 
                    ('alice_admin', 'alice@example.com', 1),  -- Administrator
                    ('bob_dev', 'bob@example.com', 2),        -- Standard User
                    ('charlie_guest', 'charlie@example.com', 3); -- Guest

                -- ============================================================================
                -- 3. Create Tasks Table & Insert Data
                -- ============================================================================
                CREATE TABLE dbo.Tasks (
                    ID INT IDENTITY(1,1) CONSTRAINT PK_Tasks PRIMARY KEY,
                    UserID INT NOT NULL,
                    TaskDescription NVARCHAR(255) NOT NULL,
                    IsCompleted BIT NOT NULL DEFAULT 0,
                    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
                    CONSTRAINT FK_Tasks_Users FOREIGN KEY (UserID) 
                        REFERENCES dbo.Users(ID)
                );

                INSERT INTO dbo.Tasks (UserID, TaskDescription, IsCompleted)
                VALUES 
                    (1, 'Configure system environment variables', 1),
                    (1, 'Review security logs for Q2', 0),
                    (2, 'Fix layout issue on the login page', 0),
                    (2, 'Write unit tests for the authentication module', 1),
                    (3, 'Submit user feedback form', 0);

                -- ============================================================================
                -- Quick Verification Query
                -- ============================================================================
                SELECT 
                    t.ID AS TaskID,
                    t.TaskDescription,
                    t.IsCompleted,
                    u.UserName,
                    ut.Name AS UserType
                FROM dbo.Tasks t
                INNER JOIN dbo.Users u ON t.UserID = u.ID
                INNER JOIN dbo.UserTypes ut ON u.UserTypeID = ut.ID;
                """;

            await sqlSvc.OpenConnectionAsync();
            await sqlSvc.ExecuteAsync(sql);
            await sqlSvc.CloseConnectionAsync();
        }

        internal static void AssertUsers(DataTable data)
        {
            Assert.True(data.Rows.Count == 3);

            Assert.Equal("alice_admin", data.Rows[0]["UserName"]);
            Assert.Equal("bob_dev", data.Rows[1]["UserName"]);
            Assert.Equal("charlie_guest", data.Rows[2]["UserName"]);

            Assert.Equal(1, data.Rows[0]["UserTypeID"]);
            Assert.Equal(2, data.Rows[1]["UserTypeID"]);
            Assert.Equal(3, data.Rows[2]["UserTypeID"]);
        }

        internal static void AssertUserInserted(DataTable data)
        {
            Assert.True(data.Rows.Count == 4);

            var insertedRow = data.Rows[3];

            Assert.Equal("dave_new", insertedRow["UserName"]);
            Assert.Equal("dave@example.com", insertedRow["Email"]);
            Assert.Equal(2, insertedRow["UserTypeID"]);
        }

        internal static void AssertUserUpdated(DataTable data)
        {
            Assert.True(data.Rows.Count == 3);

            var updatedRow = data.Rows[0];

            Assert.Equal("alice_updated", updatedRow["UserName"]);
            Assert.Equal("alice.updated@example.com", updatedRow["Email"]);
        }

        internal static void AssertUserDeleted(DataTable data)
        {
            Assert.True(data.Rows.Count == 2);

            Assert.Equal("bob_dev", data.Rows[0]["UserName"]);
            Assert.Equal("charlie_guest", data.Rows[1]["UserName"]);
        }
    }
}
