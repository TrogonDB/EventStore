using System;
using System.Linq;
using EventStore.Plugins.Authorization;

namespace EventStore.Core.Authorization {
	public class AuthorizationProviderFactory {
		private readonly Func<AuthorizationProviderFactoryComponents, IAuthorizationProviderFactory>
			_authorizationProviderFactory;

		public AuthorizationProviderFactory(
			Func<AuthorizationProviderFactoryComponents, IAuthorizationProviderFactory>
				authorizationProviderFactory) {
			_authorizationProviderFactory = authorizationProviderFactory;
		}

		public IAuthorizationProviderFactory GetFactory(
			AuthorizationProviderFactoryComponents authorizationProviderFactoryComponents) =>
			_authorizationProviderFactory(authorizationProviderFactoryComponents);
	}

	public class PolicySelectorsFactory(params IPolicySelectorFactory[] policySelectorFactories) {
		public IPolicySelector[] Create(
			AuthorizationProviderFactoryComponents authorizationProviderFactoryComponents) =>
			policySelectorFactories
					.Select(
						p => p.Create(authorizationProviderFactoryComponents.MainQueue))
					.ToArray();
	}
}
