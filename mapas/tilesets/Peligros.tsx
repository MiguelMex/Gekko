<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.10" tiledversion="1.12.2" name="Peligros" tilewidth="16" tileheight="16" tilecount="8" columns="4">
 <properties>
  <property name="collision_layer_1" value="2"/>
 </properties>
 <image source="peligros.png" width="64" height="32"/>
 <tile id="0">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0.173913" y="12.1304">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,0 3.04348,-4.52174 7.47826,-9.43478 12.4348,-4.82609 15.6957,-0.26087"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="1">
  <objectgroup draworder="index" id="3">
   <object id="2" x="15.913" y="3.95652" rotation="180">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,0 3.04348,-4.52174 7.47826,-9.43478 12.4348,-4.82609 15.6957,-0.26087"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="2">
  <objectgroup draworder="index" id="2">
   <object id="1" x="4" y="0.173913" rotation="90">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,0 3.04348,-4.52174 7.47826,-9.43478 12.4348,-4.82609 15.6957,-0.26087"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="3">
  <objectgroup draworder="index" id="2">
   <object id="1" x="13" y="15.7826" rotation="-90">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,0 3.04348,-4.52174 7.47826,-9.43478 12.4348,-4.82609 15.6957,-0.26087"/>
   </object>
  </objectgroup>
 </tile>
</tileset>
