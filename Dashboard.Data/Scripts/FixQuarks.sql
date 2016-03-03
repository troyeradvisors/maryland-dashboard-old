use MarylandHome


/* clear domicillary and apartment_villas (very high patient days counts that causes 700% occupancy) */
update routineservicerevenue
set comprehensivecarecustom2name = '0',
comprehensivecarecustom2patientdays = 0,
comprehensivecarecustom2revenue = 0,
comprehensivecarecustom3name = '0',
comprehensivecarecustom3patientdays = 0,
comprehensivecarecustom3revenue = 0
where pin = 267510200


update RoutineServiceRevenue
SET 
	ComprehensiveCareMedicaidPatientDays += ComprehensiveCareCustom1PatientDays,
	ComprehensiveCareMedicaidRevenue += ComprehensiveCareCustom1Revenue,
	ComprehensiveCareCustom1Name = '0',
	ComprehensiveCareCustom1PatientDays = 0,
	ComprehensiveCareCustom1Revenue = 0
where ComprehensiveCareCustom1Name like '%medicaid%'

update RoutineServiceRevenue
SET 
	ComprehensiveCareMedicaidPatientDays = ComprehensiveCareMedicaidPatientDays + ComprehensiveCareCustom2PatientDays,
	ComprehensiveCareMedicaidRevenue = ComprehensiveCareMedicaidRevenue + ComprehensiveCareCustom2Revenue,
	ComprehensiveCareCustom2Name = '0',
	ComprehensiveCareCustom2PatientDays = 0,
	ComprehensiveCareCustom2Revenue = 0
where ComprehensiveCareCustom2Name like '%medicaid%'

update RoutineServiceRevenue
SET 
	ComprehensiveCareMedicaidPatientDays = ComprehensiveCareMedicaidPatientDays + ComprehensiveCareCustom3PatientDays,
	ComprehensiveCareMedicaidRevenue = ComprehensiveCareMedicaidRevenue + ComprehensiveCareCustom3Revenue,
	ComprehensiveCareCustom3Name = '0',
	ComprehensiveCareCustom3PatientDays = 0,
	ComprehensiveCareCustom3Revenue = 0
where ComprehensiveCareCustom3Name like '%medicaid%'



update RoutineServiceRevenue
SET 
	ComprehensiveCareMedicarePatientDays = (ComprehensiveCareMedicarePatientDays + ComprehensiveCareCustom1PatientDays),
	ComprehensiveCareMedicareRevenue = ComprehensiveCareMedicareRevenue + ComprehensiveCareCustom1Revenue,
	ComprehensiveCareCustom1Name = '0',
	ComprehensiveCareCustom1PatientDays = 0,
	ComprehensiveCareCustom1Revenue = 0
where ComprehensiveCareCustom1Name like '%medicare%'

update RoutineServiceRevenue
SET 
	ComprehensiveCareMedicarePatientDays = ComprehensiveCareMedicarePatientDays + ComprehensiveCareCustom2PatientDays,
	ComprehensiveCareMedicareRevenue = ComprehensiveCareMedicareRevenue + ComprehensiveCareCustom2Revenue,
	ComprehensiveCareCustom2Name = '0',
	ComprehensiveCareCustom2PatientDays = 0,
	ComprehensiveCareCustom2Revenue = 0
where ComprehensiveCareCustom2Name like '%medicare%'

update RoutineServiceRevenue
SET 
	ComprehensiveCareMedicarePatientDays = ComprehensiveCareMedicarePatientDays + ComprehensiveCareCustom3PatientDays,
	ComprehensiveCareMedicareRevenue = ComprehensiveCareMedicareRevenue + ComprehensiveCareCustom3Revenue,
	ComprehensiveCareCustom3Name = '0',
	ComprehensiveCareCustom3PatientDays = 0,
	ComprehensiveCareCustom3Revenue = 0
where ComprehensiveCareCustom3Name like '%medicare%'


use marylandhome

drop table Summary
drop table CountyAverage
drop table StateAverage
select * into Summary from vSummary
select * into CountyAverage from vCountyAverage
select * into StateAverage from vStateAverage



select StatePIN,  RoutineServiceRevenue.* from RoutineServiceRevenue inner join Description on Routineservicerevenue.PIN = description.pin where ComprehensiveCareCustom1Name like '%medicaid%' or ComprehensiveCareCustom2Name like '%medicaid%' or ComprehensiveCareCustom3Name like '%medicaid%' or ComprehensiveCareCustom1Name like '%medicare%' or ComprehensiveCareCustom2Name like '%medicare%' or ComprehensiveCareCustom3Name like '%medicare%'
