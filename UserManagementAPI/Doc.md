# Activity 1

For the first activity, Copilot helped me write the initial version of th the CRUD endpoints.
It was a very simple version of them, but it sped up the process a lot.

I also used it to write the UserManagementAPI.http file to test the endpoints.

# Activiy 2

For this activity, Copilot helped me realize which validations where necessary for the
user input fields (mail, name, last name and ids). It also helped me with the handling of some errors when interacting whith the dictionary.

Once that was done, i asked Copilot about possible performance issues and it pointed out that a
dictionary could cause trouble if more than one user tried to access it at the same time. It suggested switching to a concurrent dictionary, which i did.

In this activity i also had some troubles with ports. I couldn't run the app because the port
was occupied, and Copilot helped me realize what was the problem and how to fix it.


# Activity 3

For this activity, Copilot helped me set up the logging middleware where i used HttpLogging. It
also helped me with implementing error handling middleware. I ended up using a global 
exception handler to catch any unexpected errors in the endpoints. For the authentication
middleware, it helped me with correcting the http requests so that they would work with the 
new authentication middleware and it explained me in detail how the simulated authentication
worked.

Finally it helped me with checking and confirmig the order of the middleware was the correct one.