#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NativeUI;
using FTOptix.NetLogic;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Recipe;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.CoreBase;
using FTOptix.Core;

using System.Linq;
using System.Collections.Generic;
#endregion

public class RecipesAuditLogic : BaseNetLogic
{

	//Recipe Status
	//-1 not created
	//0 new
	//1 approved
	//2
	private object[,] resultSet;
	private string[] header;
	private Store recipeStore;
	private string recipeTable;

	public override void Start()
    {
		AddTranslations();
	}

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

	[ExportMethod]
	public void CreateNewRecipe(string recipeName, NodeId recipePage)
	{
		NodeId recipeSchema = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).NodeId;

		recipeStore = InformationModel.Get<Store>(InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).Store);
		recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).TableName;
		if (recipeTable is null)
			recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).BrowseName;

		try
		{
			if (String.IsNullOrEmpty(recipeName))
				throw new Exception(GetLocalizedTextString("RecipesEditorRecipeNameEmpty"));

			if (recipeSchema == null || recipeSchema == NodeId.Empty)
				throw new Exception(GetLocalizedTextString("RecipesEditorRecipeSchemaNodeIdEmpty"));

			var schema = GetRecipeSchema(recipeSchema);
			var store = GetRecipeStore(schema);
			var editModel = GetEditModel(schema);

			// Create recipe
			schema.CreateStoreRecipe(recipeName);

			// Save Recipe
			schema.CopyToStoreRecipe(editModel.NodeId, recipeName, CopyErrorPolicy.BestEffortCopy);

			var comboBox = GetComboBox(recipePage);
			comboBox.Refresh();

			SetRecipeStatus(recipeName, 0);
			InformationModel.Get(recipePage).GetVariable("ActualRecipeStatus").Value = 0;

			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorRecipe")} {recipeName} {GetLocalizedTextString("RecipesEditorCreatedAndSaved")}", recipePage);
		}
		catch (Exception e)
		{
			Log.Error("CreateRecipe", e.Message);
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorErrorSavingRecipe")} {e.Message}", recipePage);
		}
	}

	[ExportMethod]
	public void ModifyRecipe(string recipeName, NodeId recipePage)
	{
		NodeId recipeSchema = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).NodeId;

		recipeStore = InformationModel.Get<Store>(InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).Store);
		recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).TableName;
		if (recipeTable is null)
			recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).BrowseName;

		try
		{
			if (String.IsNullOrEmpty(recipeName))
				throw new Exception(GetLocalizedTextString("RecipesEditorRecipeNameEmpty"));

			if (recipeSchema == null || recipeSchema == NodeId.Empty)
				throw new Exception(GetLocalizedTextString("RecipesEditorRecipeSchemaNodeIdEmpty"));

			var schema = GetRecipeSchema(recipeSchema);
			var store = GetRecipeStore(schema);
			var editModel = GetEditModel(schema);

			// Save Recipe
			schema.CopyToStoreRecipe(editModel.NodeId, recipeName, CopyErrorPolicy.BestEffortCopy);

			SetRecipeStatus(recipeName, 0);
			InformationModel.Get(recipePage).GetVariable("ActualRecipeStatus").Value = 0;

			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorRecipe")} {recipeName} {GetLocalizedTextString("RecipesEditorSaved")}", recipePage);
			return;
		}
		catch (Exception e)
		{
			Log.Error("ModifyRecipe", e.Message);
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorErrorSavingRecipe")} {e.Message}", recipePage);
		}
	}

	[ExportMethod]
	public void ApproveRecipe(string recipeName, NodeId recipePage)
	{
		NodeId recipeSchema = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).NodeId;

		recipeStore = InformationModel.Get<Store>(InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).Store);
		recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).TableName;
		if (recipeTable is null)
			recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).BrowseName;

		try
		{
			SetRecipeStatus(recipeName, 1);
			InformationModel.Get(recipePage).GetVariable("ActualRecipeStatus").Value = 1;
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorRecipe")} {recipeName} {GetLocalizedTextString("RecipesEditorApproved")}", recipePage);
			return;
		}
		catch (Exception e)
		{
			Log.Error("ApproveRecipe", e.Message);
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorErrorSavingRecipe")} {e.Message}", recipePage);
		}
	}

	[ExportMethod]
	public void ArchiveRecipe(string recipeName, NodeId recipePage)
	{
		NodeId recipeSchema = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).NodeId;

		recipeStore = InformationModel.Get<Store>(InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).Store);
		recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).TableName;
		if (recipeTable is null)
			recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).BrowseName;

		try
		{
			SetRecipeStatus(recipeName, 3);
			InformationModel.Get(recipePage).GetVariable("ActualRecipeStatus").Value = 3;
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorRecipe")} {recipeName} {GetLocalizedTextString("RecipesEditorArchived")}", recipePage);
			return;
		}
		catch (Exception e)
		{
			Log.Error("ArchiveRecipe", e.Message);
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorErrorSavingRecipe")} {e.Message}", recipePage);
		}
	}

	[ExportMethod]
	public void ApplyRecipe(string recipeName, NodeId recipePage)
	{
		NodeId recipeSchema = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).NodeId;

		recipeStore = InformationModel.Get<Store>(InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).Store);
		recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).TableName;
		if (recipeTable is null)
			recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).BrowseName;

		SetRecipeStatus(recipeName, 2);
		InformationModel.Get(recipePage).GetVariable("ActualRecipeStatus").Value = 2;
	}

	[ExportMethod]
	public void DeleteRecipe(string recipeName, NodeId recipePage)
	{
		NodeId recipeSchema = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).NodeId;

		recipeStore = InformationModel.Get<Store>(InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).Store);
		recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).TableName;
		if (recipeTable is null)
			recipeTable = InformationModel.Get<RecipeSchema>(InformationModel.Get(recipePage).GetVariable("RecipeSchema").Value).BrowseName;

		try
		{
			if (String.IsNullOrEmpty(recipeName))
				throw new Exception(GetLocalizedTextString("RecipesEditorRecipeNameEmpty"));

			if (recipeSchema == null || recipeSchema == NodeId.Empty)
				throw new Exception(GetLocalizedTextString("RecipesEditorRecipeSchemaNodeIdEmpty"));

			var schema = GetRecipeSchema(recipeSchema);
			var store = GetRecipeStore(schema);
			var editModel = GetEditModel(schema);

			// Save Recipe
			schema.DeleteStoreRecipe(recipeName);
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorRecipe")} {recipeName} {GetLocalizedTextString("RecipesEditorDeleted")}", recipePage);

			var comboBox = GetComboBox(recipePage);
			comboBox.Refresh();

			return;
		}
		catch (Exception e)
		{
			Log.Error("DeleteRecipe", e.Message);
			SetOutputMessage($"{GetLocalizedTextString("RecipesEditorErrorSavingRecipe")} {e.Message}", recipePage);
		}
	}

	private void SetRecipeStatus(string recipeName, int status)
    {
		recipeStore.Query($"UPDATE {recipeTable} SET Status = {status} WHERE Name = '{recipeName}'", out header, out resultSet);
	}

	private RecipeSchema GetRecipeSchema(NodeId recipeSchemaNodeId)
	{
		// Get RecipeSchema node from its NodeId
		var recipeSchemaNode = InformationModel.GetObject(recipeSchemaNodeId);
		if (recipeSchemaNode == null)
			throw new Exception($"{GetLocalizedTextString("RecipesEditorRecipeNotFound")} {recipeSchemaNodeId}");

		// Check that it is actually a RecipeSchema
		var schema = recipeSchemaNode as RecipeSchema;
		if (schema == null)
			throw new Exception($"{recipeSchemaNode.BrowseName} {GetLocalizedTextString("RecipesEditorNotARecipe")}");

		return schema;
	}

	private Store GetRecipeStore(RecipeSchema schema)
	{
		// Check if the store is set
		if (schema.Store == NodeId.Empty)
			throw new Exception($"{GetLocalizedTextString("RecipesEditorStoreOfSchema")} {schema.BrowseName} {GetLocalizedTextString("RecipesEditorNotSet")}");

		// Get store node
		var storeNode = InformationModel.GetObject(schema.Store);
		if (storeNode == null)
			throw new Exception($"{GetLocalizedTextString("RecipesEditorStore")} {schema.Store} {GetLocalizedTextString("RecipesEditorNotFound")}");

		// Check that it is actually a store
		var store = storeNode as Store;
		if (store == null)
			throw new Exception($"{GetLocalizedTextString("RecipesEditorStore")} {store.BrowseName} {GetLocalizedTextString("RecipesEditorNotAStore")}");

		return store;
	}

	private IUANode GetEditModel(RecipeSchema schema)
	{
		var editModel = schema.Get("EditModel");
		if (editModel == null)
			throw new Exception($"{GetLocalizedTextString("RecipesEditorEditModelOfSchema")} {schema.BrowseName} {GetLocalizedTextString("RecipesEditorNotFound")}");

		return editModel;
	}

	private bool RecipeExistsInStore(Store store, RecipeSchema schema, string recipeName)
	{
		// Perform query on the store in order to check if the recipe already exists
		object[,] resultSet;
		string[] header;
		var tableName = !String.IsNullOrEmpty(schema.TableName) ? schema.TableName : schema.BrowseName;
		store.Query("SELECT * FROM \"" + tableName + "\" WHERE Name LIKE \'" + recipeName + "\'", out header, out resultSet);
		var rowCount = resultSet != null ? resultSet.GetLength(0) : 0;
		return rowCount > 0;
	}

	private ComboBox GetComboBox(NodeId recipePage)
	{
		// Find ComboBox
		var comboBoxNode = InformationModel.Get(recipePage).Get("RecipesComboBox");
		if (comboBoxNode == null)
			throw new Exception($"{GetLocalizedTextString("RecipesEditorRecipesComboBoxNotFound")}");

		// Check that it is actually a ComboBox
		ComboBox comboBox = comboBoxNode as ComboBox;
		if (comboBox == null)
			throw new Exception($"{comboBoxNode.BrowseName} {GetLocalizedTextString("RecipesEditorNotAComboBox")}");

		return comboBox;
	}

	private void SetOutputMessage(string message, NodeId recipePage)
	{
		var outputMessageLabelNode = InformationModel.Get(recipePage).Get("OutputMessage");
		if (outputMessageLabelNode == null)
			throw new Exception($"{GetLocalizedTextString("RecipesEditorOutputMessageLabelNotFound")}");

		var outputMessageLogic1 = outputMessageLabelNode.GetObject("RecipesEditorOutputMessageLogic");
		outputMessageLogic1.ExecuteMethod("SetOutputMessage", new object[] { message });
	}

	private string GetLocalizedTextString(string textId)
	{
		var localizedText = new LocalizedText(textId);
		var stringFound = InformationModel.LookupTranslation(localizedText).Text;

		if (string.IsNullOrEmpty(stringFound)) // fallback to en-US
			return InformationModel.LookupTranslation(localizedText, new List<string>() { "en-US" }).Text;

		return stringFound;
	}

	private void AddTranslationIfNeeded(string key, string it, string en)
	{
		var alreadyExistsTranslation = !string.IsNullOrEmpty(InformationModel.LookupTranslation(new LocalizedText(key)).Text);
		if (alreadyExistsTranslation)
			return;

		string englishLocale = "en-US";
		if (Project.Current.Locales.Contains(englishLocale))
		{
			var localizedTextEnglish = new LocalizedText(Project.Current.NodeId.NamespaceIndex, key, en, englishLocale);
			InformationModel.AddTranslation(localizedTextEnglish);
		}

		string italianLocale = "it-IT";
		if (Project.Current.Locales.Contains(italianLocale))
		{
			var localizedTextItalian = new LocalizedText(Project.Current.NodeId.NamespaceIndex, key, it, italianLocale);
			InformationModel.SetTranslation(localizedTextItalian);
		}
	}

	private void AddTranslations()
	{
		AddTranslationIfNeeded("RecipesEditorRecipeNameEmpty", "Il nome della ricetta è vuoto", "Recipe name is empty");
		AddTranslationIfNeeded("RecipesEditorRecipeSchemaNodeIdEmpty", "Il nodeId dello schema della ricetta è vuoto", "Recipe schema nodeId is empty");
		AddTranslationIfNeeded("RecipesEditorRecipe", "Ricetta", "Recipe");
		AddTranslationIfNeeded("RecipesEditorSaved", "salvata", "saved");
		AddTranslationIfNeeded("RecipesEditorCreatedAndSaved", "creata e salvata", "created and saved");
		AddTranslationIfNeeded("RecipesEditorApproved", "approvata", "approved");
		AddTranslationIfNeeded("RecipesEditorArchived", "archiviata", "archived");
		AddTranslationIfNeeded("RecipesEditorDeleted", "cancellata", "deleted");
		AddTranslationIfNeeded("RecipesEditorErrorSavingRecipe", "Errore nel salvataggio della ricetta:", "Error saving recipe:");
		AddTranslationIfNeeded("RecipesEditorRecipeNotFound", "Ricetta non trovata, nodeId:", "Recipe not found, nodeId:");
		AddTranslationIfNeeded("RecipesEditorNotARecipe", "non è una ricetta", "is not a recipe");
		AddTranslationIfNeeded("RecipesEditorStoreOfSchema", "Lo store dello schema", "Store of schema");
		AddTranslationIfNeeded("RecipesEditorNotSet", "non è impostato", "is not set");
		AddTranslationIfNeeded("RecipesEditorStore", "Store", "Store");
		AddTranslationIfNeeded("RecipesEditorNotFound", "non trovato", "not found");
		AddTranslationIfNeeded("RecipesEditorNotAStore", "non è uno store", "is not a store");
		AddTranslationIfNeeded("RecipesEditorEditModelOfSchema", "EditModel dello schema", "EditModel of schema");
		AddTranslationIfNeeded("RecipesEditorRecipesComboBoxNotFound", "Nodo RecipesComboBox non trovato", "RecipesComboBox node not found");
		AddTranslationIfNeeded("RecipesEditorNotAComboBox", "non è una comboBox", "is not a comboBox");
		AddTranslationIfNeeded("RecipesEditorOutputMessageLabelNotFound", "Etichetta OutputMessage non trovata", "OutputMessage label not found");
	}
}
