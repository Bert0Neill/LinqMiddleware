-- original
SELECT `l`.`Address`, `l`.`CodeName`, `l`.`ExternalRef`, `l`.`Height`, `l`.`ID`, `l`.`IsActive`, `l`.`ProjectRef`, `l`.`SecurityType`, `l`.`StartDate`
FROM `LargeTable` AS `l`
WHERE `l`.`ID` IN (834542, 3232719, 2900545, 2197762, 1259320, 373650, 296790, 2297640, 1138753, 2725272) AND (`l`.`ProjectRef` NOT IN ('Finn', 'Harps', 'FC') OR (`l`.`ProjectRef` IS NULL))

-- IN & INT
SELECT * 
FROM `LargeTable` AS `l`
WHERE not JSON_CONTAINS('[375496, 1050391, 406672, 2484611]', CAST(`l`.`ID` AS JSON))
LIMIT 10;

-- NOT IN & Varchar
SELECT 
    `l`.`Address`, 
    `l`.`CodeName`, 
    `l`.`ExternalRef`, 
    `l`.`Height`, 
    `l`.`ID`, 
    `l`.`IsActive`, 
    `l`.`ProjectRef`, 
    `l`.`SecurityType`, 
    `l`.`StartDate`
FROM 
    `LargeTable` AS `l`
WHERE 
    JSON_CONTAINS('[375496, 1050391, 406672, 2484611, 2505407, 479420, 2090183, 1177688, 2966061, 514758]', CAST(`l`.`ID` AS JSON))
    AND 
    (

         -- JSON_SEARCH('[ "Finn", "Harps", "FC" ]', 'one', `l`.`ProjectRef`) IS NULL or         `l`.`ProjectRef` IS NULL
         NOT JSON_CONTAINS('["Finn", "Harps", "FC"]', JSON_QUOTE(`l`.`ProjectRef`)) or         `l`.`ProjectRef` IS NULL
    );
