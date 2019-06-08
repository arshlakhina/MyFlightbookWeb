﻿using MyFlightbook.Templates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

/******************************************************
 * 
 * Copyright (c) 2019 MyFlightbook LLC
 * Contact myflightbook-at-gmail.com for more information
 *
*******************************************************/

public partial class Controls_mfbSelectTemplates : System.Web.UI.UserControl
{
    public event EventHandler<PropertyTemplateEventArgs> TemplateSelected;
    public event EventHandler<PropertyTemplateEventArgs> TemplateUnselected;
    public event EventHandler<EventArgs> TemplatesReady;

    private const string szvsActive = "vsActiveTemplates";

    #region Properties
    /// <summary>
    /// The set of templates that are currently selected
    /// </summary>
    protected HashSet<int> ActiveTemplates
    {
        get
        {
            if (ViewState[szvsActive] == null)
                ViewState[szvsActive] = new HashSet<int>();
            return (HashSet<int>)ViewState[szvsActive];
        }
    }

    /// <summary>
    /// The templates to display
    /// </summary>
    public IEnumerable<TemplateCollection> GroupedTemplates { get; set; }

    protected IEnumerable<PropertyTemplate> UserTemplates { get; set; }
    #endregion

    public void AddTemplate(int id)
    {
        ActiveTemplates.Add(id);
        Refresh();
    }

    public void AddTemplates(IEnumerable<PropertyTemplate> rgpt)
    {
        if (rgpt == null)
            return;
        foreach (PropertyTemplate pt in rgpt)
            ActiveTemplates.Add(pt.ID);
        Refresh();
    }

    public void RemoveTemplate(int id)
    {
        ActiveTemplates.Remove(id);
        Refresh();
    }

    public void Refresh()
    {
        rptTemplates.DataSource = GroupedTemplates;
        rptTemplates.DataBind();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        UserTemplates = UserPropertyTemplate.TemplatesForUser(Page.User.Identity.Name);
        GroupedTemplates = TemplateCollection.GroupTemplates(UserTemplates);

        if (!IsPostBack)
        {
            Refresh();
            if (TemplatesReady != null)
                TemplatesReady(this, new EventArgs());
        }
    }

    protected void ckActive_CheckedChanged(object sender, EventArgs e)
    {
        if (sender == null)
            throw new ArgumentNullException("sender");

        CheckBox ck = sender as CheckBox;

        HiddenField h = (HiddenField) ck.NamingContainer.FindControl("hdnID");

        int id = Convert.ToInt32(h.Value, CultureInfo.InvariantCulture);

        if (ck.Checked)
        {
            ActiveTemplates.Add(id);
            Refresh();
            if (TemplateSelected != null)
                TemplateSelected(this, new PropertyTemplateEventArgs(UserTemplates.FirstOrDefault(pt => pt.ID == id)));
        }
        else
        {
            ActiveTemplates.Remove(id);
            Refresh();
            if (TemplateUnselected != null)
                TemplateUnselected(this, new PropertyTemplateEventArgs(id));
        }
    }
}