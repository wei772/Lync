using Microsoft.Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Service
{
	public class ContactService
	{
		private LyncService _lyncService;

		internal ContactManager ContactManager
		{
			get
			{
				return _lyncService.Client.ContactManager;
			}

		}

		private static ContactService _instance;
		public static ContactService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ContactService();
				}
				return _instance;
			}

		}

		private ContactService()
		{
			_lyncService = LyncService.Instance;
		}

		public Contact GetContactByUri(string sipUri)
		{
			return ContactManager.GetContactByUri(sipUri);
		}
	}
}
