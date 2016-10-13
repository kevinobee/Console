﻿using System.Linq;
using System.Management.Automation;
using Cognifide.PowerShell.Core.Extensions;
using Cognifide.PowerShell.Core.Validation;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;

namespace Cognifide.PowerShell.Commandlets.Data
{
    [Cmdlet(VerbsData.Update, "ItemReferrer")]
    public class UpdateItemReferrerCommand : BaseItemCommand
    {

        [Parameter(ParameterSetName = "Item from Pipeline, New Item", Mandatory = true)]
        [Parameter(ParameterSetName = "Item from Path, New Item", Mandatory = true)]
        [Parameter(ParameterSetName = "Item from ID, New Item", Mandatory = true)]
        public Item NewTarget { get; set; }

        [Parameter(ParameterSetName = "Item from Pipeline, RemoveLink", Mandatory = true)]
        [Parameter(ParameterSetName = "Item from Path, RemoveLink", Mandatory = true)]
        [Parameter(ParameterSetName = "Item from ID, RemoveLink", Mandatory = true)]
        public SwitchParameter RemoveLink { get; set; }

        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Item from Pipeline, New Item", Mandatory = true, Position = 0)]
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Item from Pipeline, RemoveLink", Mandatory = true, Position = 0)]
        public override Item Item { get; set; }

        [Parameter(ParameterSetName = "Item from Path, New Item", Mandatory = true)]
        [Parameter(ParameterSetName = "Item from Path, RemoveLink", Mandatory = true)]
        [Alias("FullName", "FileName")]
        public override string Path { get; set; }

        [Parameter(ParameterSetName = "Item from ID, New Item", Mandatory = true)]
        [Parameter(ParameterSetName = "Item from ID, RemoveLink", Mandatory = true)]
        public override string Id { get; set; }

        [AutocompleteSet("Databases")]
        [Parameter(ParameterSetName = "Item from ID, New Item")]
        [Parameter(ParameterSetName = "Item from ID, RemoveLink")]
        public override string Database { get; set; }

        [Alias("Languages")]
        [Parameter(ParameterSetName = "Item from Path, New Item")]
        [Parameter(ParameterSetName = "Item from ID, New Item")]
        [Parameter(ParameterSetName = "Item from Path, RemoveLink")]
        [Parameter(ParameterSetName = "Item from ID, RemoveLink")]
        public override string[] Language { get; set; }

        protected override void ProcessItem(Item linkedItem)
        {
            var linkDb = Globals.LinkDatabase;
            if (linkDb.GetReferrerCount(linkedItem) > 0)
            {
                if (NewTarget != null)
                {
                    linkDb
                        .GetReferrers(linkedItem)
                        .ToList()
                        .ForEach(link =>
                        {
                            var referer = linkedItem.Database.GetItem(link.SourceItemID, link.SourceItemLanguage,
                                link.SourceItemVersion);
                            var itemField = referer.Fields[link.SourceFieldID];
                            var field = FieldTypeManager.GetField(itemField);
                            referer.Editing.BeginEdit();
                            field.Relink(link, NewTarget);
                            referer.Editing.EndEdit();
                        });
                }
                else
                {
                    linkDb
                        .GetReferrers(linkedItem)
                        .ToList()
                        .ForEach(link =>
                        {
                            var referer = linkedItem.Database.GetItem(link.SourceItemID, link.SourceItemLanguage,
                                link.SourceItemVersion);
                            var itemField = referer.Fields[link.SourceFieldID];
                            var field = FieldTypeManager.GetField(itemField);
                            referer.Editing.BeginEdit();
                            field.RemoveLink(link);
                            referer.Editing.EndEdit(false,false);
                        });
                }
            }
        }
    }
}