# DGGruppe
## Available Termin API in .Net Core

### Controller List

#### Termine Controller with two methods:

##### GetTermine
- ###### Inputs => kalenderWoche:int, terminartID:int 
- ###### Output => AvailableTerminModel: List

##### Post Termin
- ###### Input => Termin: Object
- ###### Output => Status of storing Data into DB


### ** Important ** TODO 
1.	Currently, our API system lacks a dedicated table for managing customer (Kunden) information, which can result in scattered data storage and retrieval. If there is no specific reason to maintain the current entity structure, it is suggested to store all available customer data, such as Name and Email, in a separate table to implement Type 2 database normalization.  
 
2.	Additionally, it is suggested to have an additional table named "Contract" to store customers' contracts, with properties such as CustomerID, Date, Number, and Profit. (Need to Update Termin Table accordingly). 
 
3.	Currently, our API system does not rely on any authentication methods. To enhance security, it is recommend implementing a token-based authentication method, such as JSON Web Tokens (JWT). This will provide secure access control and mitigate common security risks such as replay attacks and session hijacking. By introducing an additional authentication method, we can ensure that only authorized users can access sensitive data and perform specific actions. 

4. Write more complex tests to check all possible conditions of Get and Post, run stress test 

5. We need to have a rererve method to manage the time duration of making appointment 
