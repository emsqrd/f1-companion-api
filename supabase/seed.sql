-- Insert 2025 F1 Season Drivers
-- Reference data - no audit fields (CreatedBy, UpdatedBy, DeletedBy) required
INSERT INTO "Drivers" 
  ("FirstName", "LastName", "Abbreviation", "CountryAbbreviation", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt", "DeletedAt")
VALUES
  -- Red Bull Racing
  ('Max', 'Verstappen', 'VER', 'NED', true, false, NOW(), NOW(), NULL),
  ('Liam', 'Lawson', 'LAW', 'NZL', true, false, NOW(), NOW(), NULL),
  
  -- Mercedes
  ('George', 'Russell', 'RUS', 'GBR', true, false, NOW(), NOW(), NULL),
  ('Andrea Kimi', 'Antonelli', 'ANT', 'ITA', true, false, NOW(), NOW(), NULL),
  
  -- Ferrari
  ('Charles', 'Leclerc', 'LEC', 'MON', true, false, NOW(), NOW(), NULL),
  ('Lewis', 'Hamilton', 'HAM', 'GBR', true, false, NOW(), NOW(), NULL),
  
  -- McLaren
  ('Lando', 'Norris', 'NOR', 'GBR', true, false, NOW(), NOW(), NULL),
  ('Oscar', 'Piastri', 'PIA', 'AUS', true, false, NOW(), NOW(), NULL),
  
  -- Aston Martin
  ('Fernando', 'Alonso', 'ALO', 'ESP', true, false, NOW(), NOW(), NULL),
  ('Lance', 'Stroll', 'STR', 'CAN', true, false, NOW(), NOW(), NULL),
  
  -- Alpine
  ('Pierre', 'Gasly', 'GAS', 'FRA', true, false, NOW(), NOW(), NULL),
  ('Jack', 'Doohan', 'DOO', 'AUS', true, false, NOW(), NOW(), NULL),
  
  -- Williams
  ('Alex', 'Albon', 'ALB', 'THA', true, false, NOW(), NOW(), NULL),
  ('Carlos', 'Sainz', 'SAI', 'ESP', true, false, NOW(), NOW(), NULL),
  
  -- RB (AlphaTauri)
  ('Yuki', 'Tsunoda', 'TSU', 'JPN', true, false, NOW(), NOW(), NULL),
  ('Isack', 'Hadjar', 'HAD', 'FRA', true, false, NOW(), NOW(), NULL),
  
  -- Kick Sauber
  ('Nico', 'Hulkenberg', 'HUL', 'GER', true, false, NOW(), NOW(), NULL),
  ('Gabriel', 'Bortoleto', 'BOR', 'BRA', true, false, NOW(), NOW(), NULL),
  
  -- Haas
  ('Esteban', 'Ocon', 'OCO', 'FRA', true, false, NOW(), NOW(), NULL),
  ('Oliver', 'Bearman', 'BEA', 'GBR', true, false, NOW(), NOW(), NULL)
ON CONFLICT DO NOTHING;
