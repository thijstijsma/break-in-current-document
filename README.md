# BreakInCurrentDocument

Some developers have a workflow which includes a lot of work with Edit & Continue.

Their workflow might look a bit like the following:

1. Run the application
2. Find the spot where the code needs to be changed
3. Signal the debugger to break
4. **Close the window that opens (often with the code that was running, not your change, sometimes nothing because it was all external code)**
5. Make a change
6. Continue the application and see if the change results in the right behavior
7. Repeat steps 3 through 6

I've written a simple extension that does step 4 automatically if you use the new orange Break All button.

![Screenshot of toolbar](https://thijstijsma.nl/wp-content/uploads/Break-In-Current-Document-Debugger-toolbar.png)

## Installation

Install the extension from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=thijstijsma.BreakInCurrentDocument).
