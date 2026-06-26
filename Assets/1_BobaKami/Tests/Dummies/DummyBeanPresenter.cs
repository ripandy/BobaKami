using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;
using UnityEngine;

namespace BobaKami.Tests
{
    internal class DummyBeanPresenter : IBeanPresenter
    {
        private readonly HashSet<int> shownBeans = new();
        
        public async ValueTask<bool> Show(int id, DirectionEnum throwDirection, CancellationToken cancellationToken = default)
        {
            Debug.Log($"Showing bean id: {id}");
            shownBeans.Add(id);
            await Task.Delay(1000, cancellationToken);
            return shownBeans.Contains(id);
        }

        public void Hide(int id)
        {
            Debug.Log($"Hide Bean id: {id}");
            shownBeans.Remove(id);
        }
    }
}