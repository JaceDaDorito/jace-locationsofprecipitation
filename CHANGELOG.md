**1.2.2**
- Added ``InstantiateArtifactPortal`` and ``InstantiateArtifactFormulaDisplay`` to instatiate objects relating to the artifact teleporter in Stage 5s. Does not come with the actual teleporter props.
- There was an undocumeted feature called ``PlayerSpawnPointController`` in version ``1.2.1`` that remains untested. Added a note to that changelog.

**1.2.1**
- Added ``SetupDLC3AccessNode``
- (Untested, please report issues) Added ``PlayerSpawnPointController`` to have more control over player spawn points.

**1.2.0**

- Updated for Alloyed Collective!

**1.1.2**

- Updated for SoTS!
- Added ``Shroom`` and ``Meridian`` jump pads. Will add the other DLC jump pads in the future but there is an issue with them saving into the scene causing GUID errors.

**1.1.1**

- Implemented nullcheck before setting flags of prefab instances in InstantiateAddressablePrefab.

**1.1.0**

- Added new GameObject Menu item called ``Risk of Rain 2``.
- New ``Geyser`` prefab option under ``Risk of Rain 2`` menu Item. Used to quickly make custom jump pads.
- Added ``InstantiateGeyserPrefab`` for reusing in game geysers.
- Added ``InstantiateLogbookPrefab`` for instantiating the logbook pickup usually found on hidden realms.
- Made ``Set Position And Rotation to Zero``, ``Use Local Position and Rotation``, and ``Refresh in Editor`` all set to true by default.
- Added ``ShaderSwap`` class with ``ConvertShader`` method.

**1.0.0**

- Initial Release!