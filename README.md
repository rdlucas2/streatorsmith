This is a static site which posts a form to a .net 5 web api to upload video files to azure blob storage, for a music competition.

Required Infrastructure:
Azure App Service (with app service plan)
Azure Storage Account

Add these variables to app settings in azure:
connString (for azure storage account)
secretcode (to ensure only those receiving competition letters can submit)
