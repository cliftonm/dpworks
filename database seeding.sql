set identity_insert SiteProfile on
insert into SiteProfile (Id, Name, Municipality, [State], ContactName, ContactPhone, ContactEmail)
	values (-1, 'empty', 'empty', 'em', 'empty', 'empty', 'empty')
set identity_insert SiteProfile off	

set identity_insert Unit on
insert into Unit (Id, SiteId, Abbr) 
	values (-1, -1, 'empty')
set identity_insert Unit off

set identity_insert Equipment on
insert into Equipment (Id, SiteId, Name, HourlyRate)
	values (-1, -1, 'empty', 0)
set identity_insert Equipment off

set identity_insert Estimate on
insert into Estimate (Id, SiteId, EstimateDate, Road)
	values (-1, -1, '8/19/1962', 'empty')
set identity_insert Estimate off

set identity_insert estimateEquipment on
insert into estimateEquipment (Id, EstimateId, EquipmentId, Hours, HourlyRate)
	values (-1, -1, -1, 0, 0)
set identity_insert estimateEquipment off

set identity_insert laborRate on
insert into LaborRate (Id, SiteId, Position, HourlyRate)
	values (-1, -1, 'empty', 0)
set identity_insert laborRate off

set identity_insert estimateLabor on
insert into EstimateLabor (Id, EstimateId, LaborId, Hours, HourlyRate)
	values (-1, -1, -1, 0, 0)
set identity_insert estimateLabor off

set identity_insert Material on
insert into Material (Id, SiteId, Name, UnitId, UnitCost)
	values (-1, -1, 'empty', -1, 0)
set identity_insert Material off

set identity_insert estimateMaterial on
insert into EstimateMaterial (Id, EstimateId, MaterialId, Quantity, UnitCost)
	values (-1, -1, -1, 0, 0)
set identity_insert estimateMaterial off

set identity_insert [User] on
insert into [User] (Id, SiteId, FirstName, LastName, RegistrationToken, Activated, Email, PasswordHash)
	values (-1, -1, 'empty', 'empty', 'empty', 0, 'empty', 'empty')
set identity_insert [User] off
