-- Drop database if it already exists
DROP DATABASE IF EXISTS LinqPerformanceDemo;

-- Create a new database instance
CREATE DATABASE LinqPerformanceDemo;

USE LinqPerformanceDemo;

-- Drop table if it already exists
DROP TABLE IF EXISTS LargeTable;

-- Create the new table
CREATE TABLE LargeTable (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    CodeName VARCHAR(50) NULL,
    ProjectRef VARCHAR(50) NULL,
    ExternalRef INT NULL,
    StartDate DATETIME NULL,
    IsActive BOOLEAN NULL,
    SecurityType CHAR(1) NULL,
    Height FLOAT NULL,
    Address NVARCHAR(100) NULL
);

-- Drop index if it already exists
DROP INDEX IF EXISTS NonClusteredIndex_SecurityType_Projectref ON LargeTable;

-- Create the index
CREATE INDEX NonClusteredIndex_SecurityType_Projectref ON LargeTable (SecurityType, ProjectRef);

-- Populate the table with random data
DELIMITER //

CREATE PROCEDURE PopulateLargeTable()
BEGIN
    DECLARE Counter INT DEFAULT 0;

    WHILE Counter < 1000000 DO
        INSERT INTO LargeTable (CodeName, ProjectRef, ExternalRef, StartDate, IsMale, SecurityType, Height, Address)
        VALUES (
            SUBSTRING(MD5(RAND()), 1, 8),  -- Random string
            SUBSTRING(MD5(RAND()), 1, 8),  -- Random string
            FLOOR(RAND() * 1000000),  -- Random integer
            DATE_ADD(NOW(), INTERVAL -FLOOR(RAND() * 365) DAY),  -- Random date within the past year
            FLOOR(RAND() * 2),  -- Random boolean
            CHAR(65 + FLOOR(RAND() * 26)),  -- Random char (A-Z)
            RAND() * 1000,  -- Random float
            SUBSTRING(MD5(RAND()), 1, 8)  -- Random string
        );

        SET Counter = Counter + 1;
    END WHILE;
END //

DELIMITER ;

-- Call the procedure to populate the table
CALL PopulateLargeTable();
