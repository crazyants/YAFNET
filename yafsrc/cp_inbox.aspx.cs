/* Yet Another Forum.net
 * Copyright (C) 2003 Bj�rnar Henden
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace yaf.cp
{
	/// <summary>
	/// Summary description for inbox.
	/// </summary>
	public class cp_inbox : BasePage
	{
		protected System.Web.UI.WebControls.HyperLink HomeLink;
		protected System.Web.UI.WebControls.HyperLink UserLink, ThisLink;
		protected System.Web.UI.WebControls.Repeater Inbox;
		protected LinkButton FromLink, DateLink, SubjectLink;
		protected HtmlImage SortSubject, SortFrom, SortDate;

		private void SetSort(string field,bool asc) 
		{
			if(ViewState["SortField"]!=null && (string)ViewState["SortField"]==field) 
			{
				ViewState["SortAsc"] = !(bool)ViewState["SortAsc"];
			} 
			else 
			{
				ViewState["SortField"] = field;
				ViewState["SortAsc"] = asc;
			}
		}

		private void SubjectLink_Click(object sender, System.EventArgs e) 
		{
			SetSort("Subject",true);
			BindData();
		}

		private void FromLink_Click(object sender, System.EventArgs e) 
		{
			if(IsSentItems)
				SetSort("ToUser",true);
			else
				SetSort("FromUser",true);
			BindData();
		}

		private void DateLink_Click(object sender, System.EventArgs e) 
		{
			SetSort("Created",false);
			BindData();
		}

		protected void DeleteSelected_Load(object sender, System.EventArgs e) 
		{
			((Button)sender).Attributes["onclick"] = String.Format("return confirm('{0}')",GetText("confirm_delete"));
		}

		private void Page_Load(object sender, System.EventArgs e)
		{
			if(!User.Identity.IsAuthenticated)
				Response.Redirect(String.Format("login.aspx?ReturnUrl={0}",Request.RawUrl));
			
			if(!IsPostBack) {
				SetSort("Created",false);
				IsSentItems = Request.QueryString["sent"]!=null;
				BindData();

				HomeLink.NavigateUrl = BaseDir;
				HomeLink.Text = ForumName;
				UserLink.NavigateUrl = "cp_profile.aspx";
				UserLink.Text = PageUserName;
				ThisLink.NavigateUrl = Request.RawUrl;
				ThisLink.Text = GetText(IsSentItems ? "sentitems" : "title");

				SubjectLink.Text = GetText("subject");
				FromLink.Text = GetText(IsSentItems ? "to" : "from");
				DateLink.Text = GetText("date");
			}
		}

		protected bool IsSentItems 
		{
			get 
			{
				return (bool)ViewState["IsSentItems"];
			}
			set 
			{
				ViewState["IsSentItems"] = value;
			}
		}

		private void BindData() {
			using(DataView dv = DB.pmessage_list(PageUserID,IsSentItems,null).DefaultView) 
			{
				dv.Sort = String.Format("{0} {1}",ViewState["SortField"],(bool)ViewState["SortAsc"] ? "asc" : "desc");
				Inbox.DataSource = dv;
				DataBind();
			}
			if(IsSentItems)
				SortFrom.Visible = (string)ViewState["SortField"] == "ToUser";
			else
				SortFrom.Visible = (string)ViewState["SortField"] == "FromUser";
			SortFrom.Src = ThemeFile((bool)ViewState["SortAsc"] ? "sort_up.gif" : "sort_down.gif");
			SortSubject.Visible = (string)ViewState["SortField"] == "Subject";
			SortSubject.Src = ThemeFile((bool)ViewState["SortAsc"] ? "sort_up.gif" : "sort_down.gif");
			SortDate.Visible = (string)ViewState["SortField"] == "Created";
			SortDate.Src = ThemeFile((bool)ViewState["SortAsc"] ? "sort_up.gif" : "sort_down.gif");
		}

		protected string FormatBody(object o) {
			DataRowView row = (DataRowView)o;
			return FormatMsg.ForumCodeToHtml(this,(string)row["Body"]);
		}

		private void Inbox_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e) {
			if(e.CommandName == "delete") {
				long nItemCount = 0;
				foreach(RepeaterItem item in Inbox.Items) 
				{
					if(((CheckBox)item.FindControl("ItemCheck")).Checked) 
					{
						DB.pmessage_delete(((Label)item.FindControl("PMessageID")).Text);
						nItemCount++;
					}
				}

				//TODO DB.pmessage_delete(e.CommandArgument);
				BindData();
				if(nItemCount==1)
					AddLoadMessage(GetText("msgdeleted1"));
				else
					AddLoadMessage(String.Format(GetText("msgdeleted2"),nItemCount));
			}
		}

		protected string GetImage(object o) 
		{
			if((bool)((DataRowView)o)["IsRead"]) 
				return ThemeFile("topic.png");
			else
				return ThemeFile("topic_new.png");
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			SubjectLink.Click += new EventHandler(SubjectLink_Click);
			FromLink.Click += new EventHandler(FromLink_Click);
			DateLink.Click += new EventHandler(DateLink_Click);
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Inbox.ItemCommand += new System.Web.UI.WebControls.RepeaterCommandEventHandler(this.Inbox_ItemCommand);
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
