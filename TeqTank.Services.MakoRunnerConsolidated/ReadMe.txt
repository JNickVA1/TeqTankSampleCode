This application no longer will support sequential OR parallel WebJobs or parallel Windows services.

	  In the case of WebJobs, this is due to the fact that a single console application, when deployed to Azure,
	  is limited to exactly one WebJob, meaning there is a on-to-one WebJob to console application relationship.
	  The runtime instructions have been changed to reflect this. The code has been left intact but will not be 
	  executed because it is prevented by runtime parameter checks when the program starts. In the future we may 
	  want to develop a controller application or shell script that can deploy multiple WebJobs. The basic logic
	  for running sequential and/or parallel WebJobs could be implemented in that controller application.

	  In the case of Windows services, there is no reason other than the constraints of time for testing and 
	  debugging for limiting the execution of Windows services to sequential operation. Since each service is run 
	  within a separate Task, we could easily implement parallelization, if time was available for testing and debugging.
	  Sequential Windows services is still supported and implemented in this application (although, due to the 
	  lack of test data, sequential operation has not been tested.)