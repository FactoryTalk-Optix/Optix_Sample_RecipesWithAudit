#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.EventLogger;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.Recipe;
using FTOptix.UI;
#endregion

public class RecipesWithAuditLogic : BaseNetLogic
{
    private IUAVariable editorMode;
    private IUAVariable actualRecipeStatus;
    private Store recipeStore;
    private string recipeTable;
    private RecipeSchema recipeSchema;
    private object[,] resultSet;
    private string[] header;
    private Button create, save, approve, archive, delete, load, apply;
    private int mode;

    public override void Start()
    {
        mode = InformationModel.GetVariable(Owner.GetAlias("Mode").NodeId).Value;
        LoadButtons();
        editorMode = Owner.GetVariable("EditMode");
        actualRecipeStatus = Owner.GetVariable("ActualRecipeStatus");
        recipeSchema = InformationModel.Get<RecipeSchema>(Owner.GetVariable("RecipeSchema").Value);
        recipeStore = InformationModel.Get<Store>(recipeSchema.Store);
        recipeTable = recipeSchema.TableName;
        if (recipeTable is null)
            recipeTable = recipeSchema.BrowseName;
        GetRecipeStatus(Owner.Get<ComboBox>("RecipesComboBox").Text);
        actualRecipeStatus.VariableChange += ActualRecipeStatus_VariableChange;
        ManageButtonVisibility();
    }

    private void ActualRecipeStatus_VariableChange(object sender, VariableChangeEventArgs e)
    {
        ManageButtonVisibility();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    private void LoadButtons()
    {
        create = Owner.Get<Button>("CreateButton");
        save = Owner.Get<Button>("SaveButton");
        approve = Owner.Get<Button>("ApproveButton");
        archive = Owner.Get<Button>("ArchiveButton");
        delete = Owner.Get<Button>("DeleteButton");
        load = Owner.Get<Button>("LoadButton");
        apply = Owner.Get<Button>("ApplyButton");
    }

    private void ManageButtonVisibility()
    {
        switch (mode)
        {
            case 0:
                create.Visible = false;
                save.Visible = false;
                approve.Visible = false;
                archive.Visible = false;
                delete.Visible = false;
                load.Visible = false;
                apply.Visible = true;
                break;
            case 1:
                if (actualRecipeStatus.Value == -1)
                {
                    create.Visible = true;
                    save.Visible = false;
                    approve.Visible = false;
                    archive.Visible = false;
                    delete.Visible = false;
                }
                else if (actualRecipeStatus.Value == 0)
                {
                    create.Visible = false;
                    save.Visible = true;
                    approve.Visible = true;
                    archive.Visible = true;
                    delete.Visible = true;
                }
                else if (actualRecipeStatus.Value == 1)
                {
                    create.Visible = false;
                    approve.Visible = false;
                    archive.Visible = true;
                    if (actualRecipeStatus.Value != 2)
                    {
                        save.Visible = true;
                        delete.Visible = true;
                    }
                    else
                    {
                        save.Visible = false;
                        delete.Visible = false;
                    }
                }
                else if (actualRecipeStatus.Value == 2)
                {
                    create.Visible = false;
                    save.Visible = false;
                    approve.Visible = false;
                    archive.Visible = true;
                    delete.Visible = false;
                }
                load.Visible = false;
                apply.Visible = false;
                break;
            case 2:
            case 3:
            case 4:
                create.Visible = false;
                save.Visible = false;
                approve.Visible = false;
                archive.Visible = false;
                delete.Visible = false;
                load.Visible = false;
                apply.Visible = false;
                break;
            default:
                break;
        }
    }

    [ExportMethod]
    public void GetRecipeStatus(string recipeName)
    {
        recipeStore.Query($"SELECT Status FROM {recipeTable} WHERE Name = '{recipeName}'", out header, out resultSet);
        int status = -1;
        if (resultSet.GetLength(0) == 1)
            status = int.Parse(resultSet[0, 0].ToString());
        actualRecipeStatus.Value = status;

    }


}
