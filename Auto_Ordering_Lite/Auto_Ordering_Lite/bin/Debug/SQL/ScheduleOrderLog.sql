SELECT
		 pvs.VendorID,
		 pv.Name as Vendor,
		 pvoc.OrderEmails as Email,
		 pvs.OrderTime as [Schedule Date/Time],
		 pvsl.Status,
		 pvsl.OrderValue,
		 pvsl.PONumber,
		 pvsl.senttime as [Sent Time]
		
		 
 FROM 
		mbo.PSVendorSchedule pvs
		 left join mbo.PSVendorScheduleLog pvsl on pvs.VendorID=pvsl.Vendorid
		 left join  dbo.ProductVendor pv on pv.ProductVendorId=pvsl.VendorId 
		 left join mbo.PSVendorOrderContacts pvoc on pvsl.VendorId =pvoc.VendorId

WHERE 

		  pvsl.SentDate=@date
