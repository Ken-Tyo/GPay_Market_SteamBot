using System;
using System.Collections;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Models.ItemInfoTemplates.AddItemInfoTemplateDtos
{
    public sealed class AddItemInfoTemplateCommand : IEnumerable<AddItemInfoTemplateValueCommand>
    {
        private readonly IEnumerable<AddItemInfoTemplateValueCommand> _data;

        public AddItemInfoTemplateCommand(IEnumerable<AddItemInfoTemplateValueCommand> data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public IEnumerator<AddItemInfoTemplateValueCommand> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    }
}
