SELECT Distinct 
 	pi.ProductItemId
	,pi.LongName
	,pi.Barcode
	,(Select SUBSTRING((SELECT ',' + pib.Barcode AS 'data()' FROM ProductItemBarcode pib where pib.ProductItemId = pi.ProductItemId FOR XML PATH('')), 0, 9999)) as AltBarcode
	,(SELECT StockQuantity FROM [dbo].[CalculatedInventory] ci WHERE CompanyBranchId='1' AND ci.ProductItemid=pi.ProductItemId)as InvJDC
	,pi.CostPrice

	  ,omoq.[MOQ]
      ,omoq.[MOQUnit]
	  ,(SELECT [Target]  FROM [mbo].[PSInventoryTargets] WHERE [BranchId]='1' and [ProductItemId]=pi.ProductItemId)as 'TargetJDC'	

FROM ProductItem pi

LEFT JOIN mbo.PSOrderingMOQ omoq on omoq.ProductItemid=pi.productitemid
LEFT JOIN [mbo].[PSVendorItemMapping] vim on pi.ProductItemId = vim.ProductItemId
LEFT JOIN ProductItemBarcode pib on pib.ProductItemId = vim.ProductItemId 
WHERE vim.ProductVendorId=@VendorId