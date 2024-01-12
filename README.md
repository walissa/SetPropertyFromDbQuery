[![Build status](https://dev.azure.com/waal/BizTalk%20Components/_apis/build/status/BizTalk%20Components/SetPropertyFromDbQuery)](https://dev.azure.com/waal/BizTalk%20Components/_build/latest?definitionId=22)

# Set Property From Db Query

Allows to set a property from the returned value of executing a SQL Server query.

The query can be a stored procedure or T-SQL, the query must return at least one record with one column.

| Parameter | Description | Type | Validation |
|-|-|-|-|
|Connection| The connection to the database, check remarks for further explanation| String| Required|
|Query| the query to be executed that returns the record holds the returned value|String| Required
|DestinationProperty| The property that the returned value will be written to|String| Required
|NoPromotion| Specifies if the property should not be promoted | Boolean | Required |
|ThrowException| Specifies if the component should throw an exception if the executed query returned no records. | Boolean |Required|
|ReturnedField| The fieldname which its value will be set to the destination property| String | Optional|
|Disabled |Set to True to disable the component, default value = False|Bool|Required|


## Remarks ##
 - Connection: the component will process the connection value in the following order:
    1. If the specified value is a connection key in the ConnectionStrings section in the configuration file.
    2. If the specified value is a system variable (Environment variable).
    3. The connection string itself.
- Query: the query can be any T-SQL query or command:
  - ``select CustomerNo, CustomerName from Customers where CustomerId=1234 ``
  - ``exec sp_GetCustomerInfoById @CustomerId=1234``
  - ``select getdate()``
  
  The component has the capability to parse context properties in the query, providing a higher level of dynamism.
  <br/>
  ``select CustomerNo, CustomerName from Customers where CustomerId={https://schemas.somecompany.com/customer-properties/customerId} ``
  <br/>Or<br/>
  ``exec sp_GetCustomers @country={https://schemas.somecompany.com/customer-properties/country},@region={https://schemas.somecompany.com/customer-properties/region}``
  
  If the property does not exist, the component will parse the property as null.
  If the query returns no records, and ThrowException is set to True, the component throws an exception; otherwise, the property will not be set.
- ReturnedField: ReturnedField is an optional parameter, if provided, the destination property will be set to the corresponding value.
  If the ReturnedField does not exist in the query result, and ThrowException is set to True, the component will throw an exception; otherwise, the property will not be set.
  if the ReturnedField is not set, the first column of the first record of the query result will be used to set the destination property.
- The query's returned value is a strongly-typed value, Make sure to specify the expected type for the destination property in the property schema.


