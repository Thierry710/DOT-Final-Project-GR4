INSERT INTO Batches (BatchId, ProductType, Quantity, Unit, OriginLocation, ProductionDate, ProducerId, Status, CreatedAt)
SELECT 'BT-20261122-001', 'Coffee Beans', 100, 'kg', 'Huye', '2026-11-20', UserId, 'Registered', GETDATE()
FROM Users WHERE Username = 'producer1';

INSERT INTO Batches (BatchId, ProductType, Quantity, Unit, OriginLocation, ProductionDate, ProducerId, Status, CreatedAt)
SELECT 'BT-20261122-002', 'Tea Leaves', 250, 'kg', 'Nyungwe', '2026-11-21', UserId, 'Registered', GETDATE()
FROM Users WHERE Username = 'producer1';

INSERT INTO Batches (BatchId, ProductType, Quantity, Unit, OriginLocation, ProductionDate, ProducerId, Status, CreatedAt)
SELECT 'BT-20261122-003', 'Maize Flour', 500, 'kg', 'Musanze', '2026-11-22', UserId, 'Registered', GETDATE()
FROM Users WHERE Username = 'producer1';
