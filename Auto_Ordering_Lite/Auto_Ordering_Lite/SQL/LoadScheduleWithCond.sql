SELECT
	pvs.VendorID,
(select name from	[dbo].[ProductVendor] pv where pv.ProductVendorId=pvs.VendorID ) AS Name,
(SELECT [OrderEmails] FROM [mbo].[PSVendorOrderContacts] pvoc where pvs.VendorId=pvoc.VendorID) AS Email,
	pvs.OrderMethod
	
FROM
	mbo.PSVendorSchedule pvs

WHERE
(	pvs.@dow='0' or pvs.[DayofMonth]=@dom)
	and
	pvs.OrderTime < convert(varchar(10), GETDATE(), 108)
	and
	pvs.VendorID NOT IN
			(

		SELECT 
			pvsl.VendorID
		FROM
			[mbo].[PSVendorScheduleLog] pvsl 
		WHERE 
			pvsl.SentDate=CONVERT(varchar,getdate(),110) and pvsl.Status='Sent'
			)