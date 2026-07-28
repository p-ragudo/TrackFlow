import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { Template } from '@/app/types/Template';
import TemplateButton from './TemplateButton';

interface TemplatesSectionProps {
    templates: Template[]
    setActiveTemplate: (template: Template) => void
    setModalVisible: (visibility: boolean) => void
}

export default function TemplatesSection({ templates, setActiveTemplate, setModalVisible }: TemplatesSectionProps) {
    return (
        <View style={styles.section}>
            {templates.length > 0 ? (
                templates.map(template => (
                    <TemplateButton 
                        key={template.id}
                        template={template}
                        setActiveTemplate={setActiveTemplate}
                        setModalVisible={setModalVisible}
                    />
                ))
            ) : (
                <Text>No templates available. Add one now.</Text>
            )}
        </View>
    )
}

const styles = StyleSheet.create({
    section: {
        gap: 10,
        marginBottom: 100
    }
})