using System.Collections.Generic;

namespace Lyrica.Services.CodePaste
{
    public interface ICodePasteRepository
    {
        UserCodePaste GetPaste(int id);

        IEnumerable<UserCodePaste> GetPastes();

        void AddPaste(UserCodePaste paste);
    }
}