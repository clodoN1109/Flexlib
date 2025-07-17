# REMOVE LIBRARY/ITEM/COMMENT/PROPERTY USE CASES

...

# LIBRARY CREATION
 
- GOAL: To create a new `library`.
- ACTORS: Personal computer user.
- PRE-CONDITIONS:
- FLOW A (CLI):
    1. The actor commands the creation of a new `library` in a selected directory with a selected name.
    2. A new `library` is created. 
- FLOW B (CLI):
    1. The actor commands to edit the library repository metadata file. 
    2. The system opens the library repository metadata file in a standard terminal editor.
    3. The actor manually adds libraries to the list.
    4. The actor commands the system to update.
    5. The system updates the local repository, fetching the necessary items from their informed origins.
- `COMPLETION`: 2/2

# ADD ITEM

- GOAL: To allow adding new items to a library.
- ACTORS: Personal computer user.
- PRE-CONDITIONS: There's a library created with Flexlib.
- FLOW A (CLI):
    1. The actor invokes flexlib command passing the library name,
      and the item's name and origin. If the name is not passed or is not valid, the leaf of the passed origin is used to name the item in the library.  
    2. Flexlib attepts to add the item and returns the result of the operation, and the relevant details in case it was not successfull.
- FLOW B (CLI):
    1. The actor commands to edit the library repository metadata file. 
    2. The system opens the library repository metadata file in a standard terminal editor.
    3. The actor manually adds items to any existing libraries.
    4. The actor commands the system to update.
    5. The system updates the local repository, fetching the necessary items from their informed origins.
- `COMPLETION`: 2/2

# CREATE/ACCESS/EDIT ITEM PROPERTY

- GOAL: To allow editing the metadata of a library item.
- ACTORS: Personal computer user.
- PRE-CONDITIONS: The actor has access to a non-empty library.
- FLOW A - CLI:
    1. The actor selects an item within a library and queries the list of its properties.
    2. The selected item's properties are listed with their values and types.
    5. The actor passes a new value to a selected  property.
    6. The system attempts to update the property value and informs the operation results.
- `COMPLETION`: 1/1 

# ADD COMMENT TO LIBRARY ITEM

- GOAL: A special library item property, given the name of comments, to reference items within or between libraries, adding information about them.
- ACTORS: Personal computer user.
- PRE-CONDITIONS: The actor has access to a non-empty library.
- FLOW A (CLI):
    1. The actor lists the existing comments for a specified item, library, or for all libraries and items.
    2. The system list the required list of comments.
    3. The actor passes a new text body the a new or existing specified library item. The text body may contain `references` to other items. 
    4. The system saves the text as a comment, linking references with existing items.
- `COMPLETION`: 1/1 

# VIEW ITEM CONTENT

- GOAL: A facility to invoke an adequate program to visualize the content of the library's items.
- ACTORS: Personal computer user.
- PRE-CONDITIONS: The actor has access to a non-empty library.
- FLOW A (CLI):
    1. The user selects an item by its name and commands its exhibition.
    2. An adequate program is invoked to exhibit the selected item. 
- `COMPLETION`: 0/1 

# LISTING ITEMS

- GOAL: List a filtered, grouped and sorted selection of items.
- ACTORS: Personal computer user.
- PRE-CONDITIONS: The actor has access to a non-empty library.
- FLOW A (CLI):
    1. The actor selects a set of filters, a grouping property and sorting rules.
    2. A corresponding list of items is exhibited as a table, showing the managed properties in a header and other properties as collumns in the table.
- `COMPLETION`: 0/1 

