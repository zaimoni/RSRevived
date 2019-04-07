import sys
import fileinput

# Use this to mitigate manual proofreading issues when adjustming GameTiles.IDs

# not really interested in parsing
nontrivial_tiles = ['FLOOR_ASPHALT',
      'FLOOR_CONCRETE',
      'FLOOR_GRASS',
      'FLOOR_OFFICE',
      'FLOOR_PLANKS',
      'FLOOR_SEWER_WATER',
      'FLOOR_TILES',
      'FLOOR_WALKWAY',
      'ROAD_ASPHALT_EW',
      'ROAD_ASPHALT_NS',
      'RAIL_EW',
      'WALL_BRICK',
      'WALL_CHAR_OFFICE',
      'WALL_HOSPITAL',
      'WALL_POLICE_STATION',
      'WALL_SEWER',
      'WALL_STONE',
      'WALL_SUBWAY',
      'RAIL_NS',
      'RAIL_SWNE',
      'RAIL_SWNE_WALL_W',
      'RAIL_SWNE_WALL_E',
      'RAIL_SENW',
      'RAIL_SENW_WALL_W',
      'RAIL_SENW_WALL_E'
]
# new tiles start at Rail_NS:   // new RS Revived 0.10.0.0
nontrivial_tile_constructor_params = {
	  'FLOOR_ASPHALT':'LIT_GRAY1, true, true',
      'FLOOR_CONCRETE':'LIT_GRAY2, true, true',
      'FLOOR_GRASS':'Color.Green, true, true',
      'FLOOR_OFFICE':'LIT_GRAY3, true, true',
      'FLOOR_PLANKS':'LIT_BROWN, true, true',
      'FLOOR_SEWER_WATER':'Color.Blue, true, true, GameImages.TILE_FLOOR_SEWER_WATER_COVER',
      'FLOOR_TILES':'LIT_GRAY2, true, true',
      'FLOOR_WALKWAY':'LIT_GRAY2, true, true',
      'ROAD_ASPHALT_EW':'LIT_GRAY1, true, true',
      'ROAD_ASPHALT_NS':'LIT_GRAY1, true, true',
      'RAIL_EW':'LIT_GRAY1, true, true',
      'WALL_BRICK':'DRK_GRAY1, false, false',
      'WALL_CHAR_OFFICE':'DRK_RED, false, false',
      'WALL_HOSPITAL':'Color.White, false, false',
      'WALL_POLICE_STATION':'Color.CadetBlue, false, false',
      'WALL_SEWER':'Color.DarkGreen, false, false',
      'WALL_STONE':'DRK_GRAY1, false, false',
      'WALL_SUBWAY':'Color.Blue, false, false',
      'RAIL_NS':'LIT_GRAY1, true, true',
      'RAIL_SWNE':'LIT_GRAY1, true, true',
      'RAIL_SWNE_WALL_W':'Color.Blue, false, false',
      'RAIL_SWNE_WALL_E':'Color.Blue, false, false',
      'RAIL_SENW':'LIT_GRAY1, true, true',
      'RAIL_SENW_WALL_W':'Color.Blue, false, false',
      'RAIL_SENW_WALL_E':'Color.Blue, false, false'
}

tile_overrides = {
	'WALL_POLICE_STATION':'WALL_STONE',
	'WALL_SUBWAY':'WALL_STONE',
}

# 0 : no triggers hit
# 1 : have seen the array backing definition request
# 2 : have seen the static forwarders start
# 3 : have seen the IDs enum start
# 4 : past IDs enum
buffering = 0
dest = open('GameTiles.cs.new','w')

with fileinput.input('Gametiles.cs') as f:
	for line in f:
		if 4<=buffering:	# past the enum, in pass-through mode
			dest.write(line)
			continue

		if 0==buffering:
			if 'public enum IDs' in line:
				buffering = 3
				continue
			if '#region static forwarders' in line:
				buffering = 2
				continue
			if 'private static readonly TileModel[] m_Models' in line:
				if '];' in line:
					dest.write('    private static readonly TileModel[] m_Models = new TileModel[]{\n')	# start function target
					dest.write('      TileModel.UNDEF,\n')
					for x in nontrivial_tiles:
						dest.write('      new TileModel(GameImages.TILE_'+(tile_overrides[x] if x in tile_overrides else x)+', '+nontrivial_tile_constructor_params[x]+') { ID = (int)IDs.'+x+' },\n')
					dest.write('    };\n')	# end function target
					continue
				buffering = 1
				continue
			dest.write(line)
			continue
		if 1==buffering:
			if '};' in line:
				buffering = 0
				dest.write('    private static readonly TileModel[] m_Models = new TileModel[]{\n')
				dest.write('      TileModel.UNDEF,\n')
				for x in nontrivial_tiles:
					dest.write('      new TileModel(GameImages.TILE_'+(tile_overrides[x] if x in tile_overrides else x)+', '+nontrivial_tile_constructor_params[x]+') { ID = (int)IDs.'+x+' },\n')
				dest.write('    };\n')
				continue
		if 2==buffering:
			if '#endregion' in line:
				buffering = 0
				dest.write('#region static forwarders\n')
				for x in nontrivial_tiles:
					dest.write('    static public TileModel '+x+' { get { return m_Models[(int)IDs.'+x+']; } }\n')
				dest.write('#endregion\n')
		if 3==buffering:
			if '}' in line:
				buffering = 4
				dest.write('    public enum IDs {\n')
				dest.write('      UNDEF = 0,\n')
				for x in nontrivial_tiles:
					if 'RAIL_NS'==x:
						dest.write('      '+x+',  // new RS Revived 0.10.0.0\n')
					else:
						dest.write('      '+x+',\n')
				dest.write('      _COUNT\n')
				dest.write('    }\n')
			continue;
